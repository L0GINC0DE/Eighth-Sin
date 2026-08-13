using System;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SkillPresentation : MonoBehaviour
{
    [SerializeField] private GameObject skillObject;
    [SerializeField] private SkillPresentationSettings settings;

    private Sequence diceSequence;
    private Vector3 initialLocalPosition;

    private void Awake()
    {
        if (skillObject == null)
            skillObject = gameObject;

        ResolveSettings();

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
        Transform skillBody = skillObject.transform.Find("Cube");
        Transform slotVisual = skillBody != null
            ? skillBody.Find("Cube")
            : null;

        if (diceSocket == null)
            diceSocket = skillObject.transform;

        Transform diceTransform = dice.transform;
        SkillPresentationSettings activeSettings = ResolveSettings();
        Quaternion surfaceLocalRotation = skillBody != null
            ? skillBody.localRotation
            : diceSocket.localRotation;
        Quaternion diceFaceUpright = Quaternion.Euler(0f, 0f, 180f);
        Quaternion insertRotation =
            surfaceLocalRotation * diceFaceUpright * savedResultRotation;
        float approachDistance = activeSettings != null
            ? activeSettings.InsertDistance
            : 0.5f;
        float insertDepth = activeSettings != null
            ? activeSettings.InsertDepth
            : 0.3f;
        float insertDuration = activeSettings != null
            ? activeSettings.InsertDuration
            : 0.7f;
        float insertedScale = activeSettings != null
            ? activeSettings.InsertedScale
            : 0.9f;
        float scaleDurationRatio = activeSettings != null
            ? activeSettings.ScaleDurationRatio
            : 0.4f;
        float insertedHoldDuration = activeSettings != null
            ? activeSettings.InsertedHoldDuration
            : 0.2f;
        float exitDistance = activeSettings != null
            ? activeSettings.ExitDistance
            : 5f;
        float exitDuration = activeSettings != null
            ? activeSettings.ExitDuration
            : 0.7f;
        Vector3 socketLocalForward =
            surfaceLocalRotation * Vector3.forward;
        Vector3 targetLocalPosition = GetTargetLocalPosition(
            diceSocket,
            skillBody,
            slotVisual,
            socketLocalForward,
            insertDepth
        );
        Vector3 startLocalPosition =
            targetLocalPosition - socketLocalForward * approachDistance;

        diceTransform.DOKill();
        diceTransform.SetParent(skillObject.transform, true);
        diceTransform.localPosition = startLocalPosition;
        diceTransform.localRotation = insertRotation;
        Vector3 targetScale = diceTransform.localScale * insertedScale;

        diceSequence?.Kill();
        diceSequence = DOTween.Sequence();
        diceSequence.Append(
            diceTransform
                .DOLocalMove(targetLocalPosition, insertDuration)
                .SetEase(Ease.InBack)
        );
        float scaleDuration = insertDuration * scaleDurationRatio;
        float scaleStartTime = insertDuration - scaleDuration;
        diceSequence.Insert(
            scaleStartTime,
            diceTransform
                .DOScale(targetScale, scaleDuration)
                .SetEase(Ease.InCubic)
        );
        diceSequence.AppendInterval(insertedHoldDuration);
        diceSequence.Append(
            transform
                .DOLocalMoveX(
                    transform.localPosition.x - exitDistance,
                    exitDuration
                )
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

    private SkillPresentationSettings ResolveSettings()
    {
        if (settings == null)
            settings = FindFirstObjectByType<SkillPresentationSettings>();

        return settings;
    }

    private Vector3 GetTargetLocalPosition(
        Transform diceSocket,
        Transform skillBody,
        Transform slotVisual,
        Vector3 surfaceLocalForward,
        float insertDepth)
    {
        if (slotVisual == null || skillBody == null)
            return diceSocket.localPosition;

        Vector3 slotCenter = skillObject.transform
            .InverseTransformPoint(slotVisual.position);
        float slotHalfDepth =
            Mathf.Abs(skillBody.localScale.z * slotVisual.localScale.z) * 0.5f;

        Vector3 slotFrontSurface =
            slotCenter - surfaceLocalForward * slotHalfDepth;

        return slotFrontSurface + surfaceLocalForward * insertDepth;
    }

}
