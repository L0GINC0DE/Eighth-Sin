using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class DiceRotation : MonoBehaviour
{
    public GameObject[] dices;
    private Sequence diceSequence;

    [SerializeField] private float rotationDuration = 1f;
    [SerializeField] private float startInterval = 0.25f;
    [SerializeField] private AnimationCurve rotationEase;
    [SerializeField] private bool enableSpacebarDebugRoll;

    private readonly List<DiceHome> diceHomes = new();

    private sealed class DiceHome
    {
        public GameObject Dice;
        public Transform Parent;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
        public bool HasBeenRolled;
    }

    private void Awake()
    {
        CacheInitialDice();
    }

    private void Update()
    {
        if (enableSpacebarDebugRoll &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RollDice(dices?.Length ?? 0);
        }
    }

    public IReadOnlyList<int> RollDice(int count)
    {
        List<int> results = new();

        if (dices == null || dices.Length == 0 || count <= 0)
            return results;

        diceSequence?.Kill();
        diceSequence = DOTween.Sequence();

        int slotCount = Mathf.Min(count, diceHomes.Count);
        for (int i = 0; i < slotCount; i++)
        {
            DiceHome diceHome = diceHomes[i];

            GameObject dice = diceHome.Dice;

            if (NeedsRefill(diceHome))
                RestoreDice(diceHome);
            else
                dice.transform.DOKill();

            diceHome.HasBeenRolled = true;

            int result = Random.Range(1, 7);
            results.Add(result);

            Vector3 resultAngle = GetResultAngle(result);
            Quaternion resultLocalRotation =
                diceHome.LocalRotation * Quaternion.Euler(resultAngle);
            ObjectDrag objectDrag = dice.GetComponent<ObjectDrag>();

            if (objectDrag != null)
                objectDrag.SetResultRotation(Quaternion.Euler(resultAngle));

            DiceValue diceValue = dice.GetComponent<DiceValue>();

            if (diceValue == null)
                diceValue = dice.AddComponent<DiceValue>();

            diceValue.SetValue(result);

            Vector3 extraSpin = new Vector3(
                Random.Range(4, 8) * 360f,
                Random.Range(4, 8) * 360f,
                Random.Range(4, 8) * 360f
            );

            Tween rotationTween = dice.transform
                .DOLocalRotate(
                    resultLocalRotation.eulerAngles + extraSpin,
                    rotationDuration,
                    RotateMode.FastBeyond360
                )
                .SetEase(rotationEase);
            Debug.Log(diceValue.Value);

            diceSequence.Insert(i * startInterval, rotationTween);
        }

        return results;
    }

    private void CacheInitialDice()
    {
        diceHomes.Clear();

        if (dices == null)
            return;

        foreach (GameObject dice in dices)
        {
            if (dice == null)
                continue;

            DiceHome home = CreateHome(dice);
            diceHomes.Add(home);
        }
    }

    private static bool NeedsRefill(DiceHome home)
    {
        return !home.HasBeenRolled ||
               !home.Dice.activeSelf ||
               home.Dice.transform.parent != home.Parent;
    }

    private static DiceHome CreateHome(GameObject dice)
    {
        Transform diceTransform = dice.transform;

        return new DiceHome
        {
            Dice = dice,
            Parent = diceTransform.parent,
            LocalPosition = diceTransform.localPosition,
            LocalRotation = diceTransform.localRotation,
            LocalScale = diceTransform.localScale
        };
    }

    private static void RestoreDice(DiceHome home)
    {
        Transform diceTransform = home.Dice.transform;
        diceTransform.DOKill();
        diceTransform.SetParent(home.Parent, false);
        diceTransform.localPosition = home.LocalPosition;
        diceTransform.localRotation = home.LocalRotation;
        diceTransform.localScale = home.LocalScale;
        home.Dice.SetActive(true);
    }

    private Vector3 GetResultAngle(int result)
    {
        return result switch
        {
            1 => new Vector3(0f, 0f, 0f),
            2 => new Vector3(-180f, 90f, 0f),
            3 => new Vector3(-90f, 0f, 0f),
            4 => new Vector3(180f, -90f, 0f),
            5 => new Vector3(90f, 0f, 0f),
            6 => new Vector3(180f, 0f, 0f),
            _ => Vector3.zero
        };
    }
}
