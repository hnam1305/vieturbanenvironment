using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class ShopOpener : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject shopRoot;        
    [SerializeField] private GameObject crosshair;       

    [Header("Tương tác")]
    [SerializeField] private Camera playerCamera;          
    [SerializeField] private float interactDistance = 6f;
    [SerializeField] private LayerMask raycastMask = ~0;

    [Header("Khoá điều khiển khi mở shop")]
    [Tooltip("Kéo script di chuyển player và script xoay camera (VD: CinemachineInputAxisController) vào đây")]
    [SerializeField] private MonoBehaviour[] disableWhileOpen;

    public bool IsOpen { get; private set; }

    private CursorLockMode _previousLockState;
    private bool _previousCursorVisible;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (shopRoot != null) shopRoot.SetActive(false);
    }

    private void Update()
    {
        if (IsOpen)
        {
            if (EscapePressed()) Close();
            return;
        }

        if (ClickPressed() && IsLookingAtThisShop())
            Open();
    }

    private bool IsLookingAtThisShop()
    {
        if (playerCamera == null) return false;
        Vector3 screenPoint = Cursor.lockState == CursorLockMode.Locked
            ? new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
            : (Vector3)PointerPosition();

        Ray ray = playerCamera.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, raycastMask, QueryTriggerInteraction.Ignore))
            return hit.collider.GetComponentInParent<ShopOpener>() == this;

        return false;
    }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;

        _previousLockState = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;

        shopRoot.SetActive(true);
        if (crosshair != null) crosshair.SetActive(false);

        SetControlsEnabled(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;

        shopRoot.SetActive(false);
        if (crosshair != null) crosshair.SetActive(true);

        SetControlsEnabled(true);
        Cursor.lockState = _previousLockState;
        Cursor.visible = _previousCursorVisible;
    }

    private void SetControlsEnabled(bool enabled)
    {
        foreach (MonoBehaviour behaviour in disableWhileOpen)
            if (behaviour != null) behaviour.enabled = enabled;
    }
    private static bool ClickPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private static bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    private static Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }
}