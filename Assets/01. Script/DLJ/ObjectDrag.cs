using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectDrag : MonoBehaviour
{
    public static GameObject Icon;
    public Quaternion ResultRotation { get; private set; } = Quaternion.identity;

    [SerializeField] private Camera targetCamera;
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 1f, 0f);

    private Vector3 _beforePosition;
    private Quaternion _beforeRotation;
    private Vector3 _dragOffset;
    private Vector3 _dragStartCameraDirection;
    private float _dragScreenDepth;
    private bool _isDragging;

    public void SetResultRotation(Quaternion resultRotation)
    {
        ResultRotation = resultRotation;
    }

    private Camera TargetCamera
    {
        get
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            return targetCamera;
        }
    }

    private void Update()
    {
        Pointer pointer = Pointer.current;

        if (pointer == null || TargetCamera == null)
            return;

        Vector2 pointerPosition = ConvertToCameraPosition(pointer.position.ReadValue());

        if (pointer.press.wasPressedThisFrame)
            TouchBeganEvent(pointerPosition);

        if (_isDragging && pointer.press.isPressed)
            TouchMovedEvent(pointerPosition);

        if (_isDragging && pointer.press.wasReleasedThisFrame)
            TouchEndedEvent(pointerPosition);

#if UNITY_EDITOR
        DrawDebugRays(pointerPosition);
#endif
    }

    private void TouchBeganEvent(Vector2 pointerPosition)
    {
        ObjectDrag selectedObject = FindDraggable(pointerPosition);

        if (selectedObject != this)
            return;

        Icon = gameObject;
        _isDragging = true;
        _beforePosition = transform.position;
        _beforeRotation = transform.rotation;
        _dragStartCameraDirection =
            (TargetCamera.transform.position - transform.position).normalized;

        Vector3 objectScreenPosition = TargetCamera.WorldToScreenPoint(transform.position);
        _dragScreenDepth = objectScreenPosition.z;

        Vector3 pointerWorldPosition = TargetCamera.ScreenToWorldPoint(
            new Vector3(pointerPosition.x, pointerPosition.y, _dragScreenDepth)
        );
        _dragOffset = transform.position - pointerWorldPosition;
    }

    private void TouchMovedEvent(Vector2 pointerPosition)
    {
        if (Icon == null)
            return;

        Vector3 worldPosition = TargetCamera.ScreenToWorldPoint(
            new Vector3(pointerPosition.x, pointerPosition.y, _dragScreenDepth)
        ) + _dragOffset;

        Icon.transform.position = Vector3.MoveTowards(
            Icon.transform.position,
            worldPosition,
            Time.deltaTime * moveSpeed
        );

        FaceCameraFromDragStart();
    }

    private void TouchEndedEvent(Vector2 pointerPosition)
    {
        if (Icon == null)
        {
            _isDragging = false;
            return;
        }

        Ray ray = TargetCamera.ScreenPointToRay(pointerPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        RaycastHit? dropTarget = null;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.transform.IsChildOf(Icon.transform))
            {
                dropTarget = hit;
                break;
            }
        }

        if (dropTarget.HasValue)
        {
            Transform hitTransform = dropTarget.Value.transform;
            SkillSystem skillSystem =
                hitTransform.GetComponentInParent<SkillSystem>();

            if (skillSystem != null)
            {
                skillSystem.InsertDice(Icon);
                Debug.Log("hit info : " + skillSystem.gameObject.name);
            }
            else
                RestoreDraggedObject();
        }
        else
            RestoreDraggedObject();

        _isDragging = false;
        Icon = null;
    }

    private void RestoreDraggedObject()
    {
        if (Icon == null)
            return;

        Icon.transform.position = _beforePosition;
        Icon.transform.rotation = _beforeRotation;
    }

    private ObjectDrag FindDraggable(Vector2 pointerPosition)
    {
        Ray ray = TargetCamera.ScreenPointToRay(pointerPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        foreach (RaycastHit hit in hits)
        {
            ObjectDrag draggable = hit.collider.GetComponentInParent<ObjectDrag>();

            if (draggable != null)
                return draggable;
        }

        return null;
    }

    private void FaceCameraFromDragStart()
    {
        Vector3 currentCameraDirection =
            TargetCamera.transform.position - Icon.transform.position;

        if (_dragStartCameraDirection.sqrMagnitude <= Mathf.Epsilon ||
            currentCameraDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Quaternion cameraDirectionDelta = Quaternion.FromToRotation(
            _dragStartCameraDirection,
            currentCameraDirection.normalized
        );

        Icon.transform.rotation = cameraDirectionDelta * _beforeRotation;
    }

    private Vector2 ConvertToCameraPosition(Vector2 screenPosition)
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return screenPosition;

        return new Vector2(
            screenPosition.x * TargetCamera.pixelWidth / Screen.width,
            screenPosition.y * TargetCamera.pixelHeight / Screen.height
        );
    }

#if UNITY_EDITOR
    private void DrawDebugRays(Vector2 pointerPosition)
    {
        if (Icon != null)
        {
            Ray dropRay = new Ray(Icon.transform.position, TargetCamera.transform.forward);
            Debug.DrawRay(dropRay.origin, dropRay.direction * 1000f, Color.red);
        }

        Ray pointerRay = TargetCamera.ScreenPointToRay(pointerPosition);
        Debug.DrawRay(pointerRay.origin, pointerRay.direction * 1000f, Color.blue);
    }
#endif
}
