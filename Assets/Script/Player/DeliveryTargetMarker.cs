using UnityEngine;

public class DeliveryTargetMarker : MonoBehaviour
{
    [SerializeField] private RectTransform marker;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Camera targetCamera;
    [SerializeField, Min(0f)] private float edgePadding = 48f;
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
        if (canvasRect == null && marker != null && marker.root != null)
        {
            canvasRect = marker.root as RectTransform;
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
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPosition, null, out localPosition))
        {
            marker.anchoredPosition = localPosition;
        }
    }
}
