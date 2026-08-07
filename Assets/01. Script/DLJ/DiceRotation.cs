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

        int rollCount = Mathf.Min(count, dices.Length);

        for (int i = 0; i < rollCount; i++)
        {
            GameObject dice = dices[i];

            if (dice == null)
                continue;

            int result = Random.Range(1, 7);
            results.Add(result);

            Vector3 resultAngle = GetResultAngle(result);
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
                    resultAngle + extraSpin,
                    rotationDuration,
                    RotateMode.FastBeyond360
                )
                .SetEase(rotationEase);

            diceSequence.Insert(i * startInterval, rotationTween);
        }

        return results;
    }

    private Vector3 GetResultAngle(int result)
    {
        return result switch
        {
            1 => new Vector3(0f, 0f, 0f),
            2 => new Vector3(0f, 90f, 0f),
            3 => new Vector3(-90f, 0f, 0f),
            4 => new Vector3(0f, -90f, 0f),
            5 => new Vector3(90f, 0f, 0f),
            6 => new Vector3(180f, 0f, 0f),
            _ => Vector3.zero
        };
    }
}
