using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class IconDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static GameObject Icon;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Transform startParent;
    private Vector2 startPosition;
    private int startSiblingIndex;

    private Transform onDragParent;
    
    [SerializeField] private float returnDuration = 0.25f;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        onDragParent = rootCanvas.transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Icon = gameObject;

        startParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;
        startSiblingIndex = transform.GetSiblingIndex();

        canvasGroup.blocksRaycasts = false;

        transform.SetParent(onDragParent, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition +=
            eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        Slot startSlotList = startParent != null
            ? startParent.GetComponentInParent<Slot>()
            : null;

        // Drop 영역이 부모를 변경하지 않았다면 원래 자리로 복귀
        if (transform.parent == onDragParent)
        {
            transform.SetParent(startParent, true);
            transform.SetSiblingIndex(startSiblingIndex);
            rectTransform
                .DOAnchorPos(startPosition, returnDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }

        startSlotList?.CompactDice();
        Icon = null;
    }
}
