using UnityEngine;
using UnityEngine.EventSystems;

public class SkillSystem : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject targetSlot;
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;

        if (draggedObject == null)
            return;

        if (!draggedObject.TryGetComponent(out IconDrag iconDrag))
            return;

        if (GetComponentInChildren<IconDrag>() != null)
            return;

        RectTransform rect =
            draggedObject.GetComponent<RectTransform>();

        rect.SetParent(targetSlot.transform, false);
        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
    }
}
