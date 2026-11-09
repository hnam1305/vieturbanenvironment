using UnityEngine;

public class DeliveryTargetMarker : MonoBehaviour
{
    [SerializeField] private RectTransform marker;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Camera targetCamera;
    [SerializeField, Min(0f)] private float edgePadding = 48f;
    [SerializeField] private float rotationOffset = -90f;
    private Transform target;
    private void Awake()
    {
        if (marker == null)
        {
            marker = transform as RectTransform;
        }
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        if (canvasRect == null && marker != null)
        {
            Canvas canvas = marker.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRect = canvas.transform as RectTransform;
            }
        }
        gameObject.SetActive(false);
    }
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        gameObject.SetActive(target != null);
    }
    private void LateUpdate()
    {
        if (target == null || marker == null || canvasRect == null || targetCamera == null)
        {
            gameObject.SetActive(false);
            return;
        }
        Vector3 screenPosition = targetCamera.WorldToScreenPoint(target.position);
        if (screenPosition.z < 0f)
        {
            screenPosition.x = Screen.width - screenPosition.x;
            screenPosition.y = Screen.height - screenPosition.y;
        }
        screenPosition.x = Mathf.Clamp(screenPosition.x, edgePadding, Screen.width - edgePadding);
        screenPosition.y = Mathf.Clamp(screenPosition.y, edgePadding, Screen.height - edgePadding);
        Vector2 localPosition;
        Camera canvasCamera = null;
        Canvas canvas = canvasRect.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = canvas.worldCamera;
        }
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPosition, canvasCamera, out localPosition))
        {
            marker.anchoredPosition = localPosition;
        }
        Vector3 directionFromCenter = screenPosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        if (directionFromCenter.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;
            marker.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }
    }
}
