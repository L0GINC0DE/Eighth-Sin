using System;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SkillPresentation : MonoBehaviour
{
    [SerializeField] private GameObject skillObject;
    [SerializeField] private float insertDistance = 0.5f;
    [SerializeField] private float insertDepth = 0.13f;
    [SerializeField] private float insertDuration = 0.7f;

    private Sequence diceSequence;
    private Vector3 initialLocalPosition;

    private void Awake()
    {
        if (skillObject == null)
            skillObject = gameObject;

        initialLocalPosition = transform.localPosition;
    }

    private void OnDestroy()
    {
        diceSequence?.Kill();
    }

    public void PlayDiceInsertion(
        GameObject dice,
        Quaternion savedResultRotation,
        Action onCompleted)
    {
        if (dice == null || skillObject == null)
        {
            onCompleted?.Invoke();
            return;
        }

        Transform diceSocket = skillObject.transform.Find("DiceSocket");
        Transform cube = skillObject.transform.Find("Cube");

        if (diceSocket == null)
            diceSocket = skillObject.transform;

        Transform diceTransform = dice.transform;
        Quaternion cubeRotation =
            cube != null ? cube.rotation : diceSocket.rotation;
        Quaternion insertRotation = cubeRotation * savedResultRotation;
        float approachDistance = Mathf.Abs(insertDistance);
        float diceHalfDepth = GetHalfExtent(dice, diceSocket.forward);
        float depth = diceHalfDepth + Mathf.Abs(insertDepth);
        Vector3 startPosition =
            diceSocket.position + diceSocket.forward * approachDistance;
        Vector3 targetPosition =
            diceSocket.position - diceSocket.forward * depth;

        diceTransform.DOKill();
        diceTransform.SetParent(null, true);
        diceTransform.position = startPosition;
        diceTransform.rotation = insertRotation;

        diceSequence?.Kill();
        diceSequence = DOTween.Sequence();
        diceSequence.Append(
            diceTransform
                .DOMove(targetPosition, insertDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    diceTransform.SetParent(transform, true);
                    diceTransform.rotation = insertRotation;
                })
        );
        diceSequence.AppendInterval(0.2f);
        diceSequence.Append(
            transform
                .DOLocalMoveX(transform.localPosition.x - 5f, insertDuration)
                .SetEase(Ease.InBack)
        );
        diceSequence.OnComplete(() => onCompleted?.Invoke());
    }

    public void ResetView()
    {
        transform.localPosition = initialLocalPosition;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private static float GetHalfExtent(GameObject dice, Vector3 direction)
    {
        Renderer[] renderers = dice.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return 0f;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 normalizedDirection = direction.normalized;
        Vector3 extents = bounds.extents;

        return Mathf.Abs(normalizedDirection.x) * extents.x +
               Mathf.Abs(normalizedDirection.y) * extents.y +
               Mathf.Abs(normalizedDirection.z) * extents.z;
    }
}
