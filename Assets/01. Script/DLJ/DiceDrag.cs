using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DiceDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform diceRect;
    private RectTransform dragParent;
    private DiceStack diceStack;
    private Vector2 grabOffset;
    private int? dragPointerId;

    private void Awake()
    {
        diceRect = GetComponent<RectTransform>();
        diceStack = GetComponentInParent<DiceStack>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || dragPointerId.HasValue)
            return;

        if (!(diceRect.parent is RectTransform parent) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.pressPosition, eventData.pressEventCamera, out Vector2 pressPosition))
            return;

        if (diceStack != null && !diceStack.TryBeginDrag(diceRect))
            return;

        dragParent = parent;
        grabOffset = diceRect.anchoredPosition - pressPosition;
        dragPointerId = eventData.pointerId;
        diceRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsActivePointer(eventData))
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragParent, eventData.position, eventData.pressEventCamera, out Vector2 pointerPosition))
            diceRect.anchoredPosition = pointerPosition + grabOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsActivePointer(eventData))
            return;

        OnDrag(eventData);
        FinishDrag(eventData);
    }

    private void OnDisable()
    {
        if (dragPointerId.HasValue)
            FinishDrag();
    }

    private void FinishDrag(PointerEventData drop = null)
    {
        dragPointerId = null;
        if (diceStack != null)
            diceStack.EndDrag(diceRect, drop);
    }

    private bool IsActivePointer(PointerEventData eventData)
    {
        return eventData.button == PointerEventData.InputButton.Left && dragPointerId == eventData.pointerId;
    }
}
