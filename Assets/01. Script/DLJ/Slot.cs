using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject[] slots;
    [SerializeField] private float moveDuration = 0.25f;

    private Sequence moveSequence;
    private Sequence layoutSequence;
    private VerticalLayoutGroup verticalLayoutGroup;

    private void AnimateSlotActivation(GameObject targetSlot)
    {
        if (targetSlot.activeSelf)
            return;

        layoutSequence?.Complete();

        RectTransform[] activeSlotRects = slots
            .Where(slot => slot != null && slot.activeSelf)
            .Select(slot => slot.GetComponent<RectTransform>())
            .Where(rect => rect != null)
            .ToArray();

        Vector2[] startPositions = activeSlotRects
            .Select(rect => rect.anchoredPosition)
            .ToArray();

        targetSlot.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            (RectTransform)transform
        );

        Vector2[] targetPositions = activeSlotRects
            .Select(rect => rect.anchoredPosition)
            .ToArray();

        if (verticalLayoutGroup != null)
            verticalLayoutGroup.enabled = false;

        for (int i = 0; i < activeSlotRects.Length; i++)
            activeSlotRects[i].anchoredPosition = startPositions[i];

        Image targetImage = targetSlot.GetComponent<Image>();
        float targetAlpha = 1f;

        if (targetImage != null)
        {
            targetAlpha = targetImage.color.a;
            targetImage.color = new Color(
                targetImage.color.r,
                targetImage.color.g,
                targetImage.color.b,
                0f
            );
        }

        layoutSequence = DOTween.Sequence()
            .SetUpdate(true);

        for (int i = 0; i < activeSlotRects.Length; i++)
        {
            RectTransform slotRect = activeSlotRects[i];
            slotRect.DOKill();

            layoutSequence.Join(
                slotRect
                    .DOAnchorPos(targetPositions[i], moveDuration)
                    .SetEase(Ease.OutCubic)
            );
        }

        if (targetImage != null)
        {
            layoutSequence.Join(
                targetImage
                    .DOFade(targetAlpha, moveDuration)
                    .SetEase(Ease.OutCubic)
            );
        }

        layoutSequence.OnComplete(() =>
        {
            if (verticalLayoutGroup != null)
                verticalLayoutGroup.enabled = true;

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                (RectTransform)transform
            );
        });
    }

    public void CompactDice()
    {
        moveSequence?.Complete();

        GameObject[] validSlots = slots
            .Where(slot => slot != null)
            .ToArray();

        IconDrag[] dice = validSlots
            .Select(slot => slot.GetComponentInChildren<IconDrag>(true))
            .Where(icon => icon != null)
            .ToArray();

        moveSequence = DOTween.Sequence()
            .SetUpdate(true);

        for (int i = 0; i < dice.Length; i++)
        {
            GameObject targetSlot = validSlots[i];
            RectTransform diceRect = dice[i].GetComponent<RectTransform>();

            targetSlot.SetActive(true);

            if (diceRect.parent == targetSlot.transform)
                continue;

            diceRect.DOKill();

            moveSequence.Join(
                diceRect
                    .DOMove(targetSlot.transform.position, moveDuration)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() =>
                    {
                        diceRect.SetParent(targetSlot.transform, false);
                        diceRect.anchoredPosition = Vector2.zero;
                        diceRect.localRotation = Quaternion.identity;
                        diceRect.localScale = Vector3.one;
                    })
            );
        }
    }
    
    private void Awake()
    {
        verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();

        foreach (var slot in slots)
        {
            if (slot != null)
                slot.gameObject.SetActive(false);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;

        if (draggedObject == null)
            return;

        if (!draggedObject.TryGetComponent(out IconDrag iconDrag))
            return;

        GameObject targetSlot = slots.FirstOrDefault(slot =>
            slot != null &&
            slot.GetComponentInChildren<IconDrag>(true) == null
        );
        
        if (targetSlot == null)
            return;
        
        AnimateSlotActivation(targetSlot);
        
        RectTransform draggedRect =
            draggedObject.GetComponent<RectTransform>();

        draggedRect.DOKill();
        draggedRect.SetParent(targetSlot.transform, true);
        draggedRect
            .DOAnchorPos(Vector2.zero, moveDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                draggedRect.anchoredPosition3D = Vector3.zero;
                draggedRect.localRotation = Quaternion.identity;
                draggedRect.localScale = Vector3.one;
            });

    }
}
