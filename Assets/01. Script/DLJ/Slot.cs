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
        
        targetSlot.gameObject.SetActive(true);
        
        RectTransform draggedRect =
            draggedObject.GetComponent<RectTransform>();

        draggedRect.SetParent(targetSlot.transform, false);
        draggedRect.anchoredPosition = Vector2.zero;
        draggedRect.localRotation = Quaternion.identity;
        draggedRect.localScale = Vector3.one;

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            (RectTransform)transform
        );
    }
}
