using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class TitleCubeRise : MonoBehaviour
{
    [SerializeField, Min(0f)] private float riseDistance = 1.1f;
    [SerializeField, Min(0.01f)] private float riseDuration = 0.35f;

    private bool hasStartedRising;

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (hasStartedRising || mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        Camera titleCamera = Camera.main;
        if (titleCamera == null)
            return;

        Ray ray = titleCamera.ScreenPointToRay(mouse.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit) || hit.collider.gameObject != gameObject)
            return;

        hasStartedRising = true;
        StartCoroutine(Rise());
    }

    private IEnumerator Rise()
    {
        Vector3 startLocalPosition = transform.localPosition;
        Vector3 endLocalPosition = startLocalPosition + Vector3.up * riseDistance;

        for (float elapsed = 0f; elapsed < riseDuration; elapsed += Time.unscaledDeltaTime)
        {
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / riseDuration);
            transform.localPosition = Vector3.Lerp(startLocalPosition, endLocalPosition, progress);
            yield return null;
        }

        transform.localPosition = endLocalPosition;
    }
}
