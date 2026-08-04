using UnityEngine;
using TMPro;

public class PlayerActions : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro useText;
    [SerializeField]
    private Transform camera;
    [SerializeField]
    private float maxUseDistance = 5f;
    [SerializeField]
    private LayerMask useLayers;
    public void OnUse()
    {
        if (Physics.Raycast(camera.position, camera.forward, out RaycastHit hit, maxUseDistance, useLayers))
        {
            if (hit.collider.TryGetComponent<Door>(out Door door))
            {
                if (door.isOpen)
                {
                    door.Close();
                }
                else
                {
                    door.Open(transform.position);
                }
            }
        }
    }
    private void Update()
    {
        if ((Physics.Raycast(camera.position, camera.forward, out RaycastHit hit, maxUseDistance, useLayers)) && hit.collider.TryGetComponent<Door>(out Door door))
        {
            if (door.isOpen)
            {
                useText.SetText("Close \"E\"");
            }
            else
            {
                useText.SetText("Open \"E\"");
            }
            useText.gameObject.SetActive(true);
            useText.transform.position = hit.point - (hit.point - camera.position).normalized * 0.01f;
            useText.transform.rotation = Quaternion.LookRotation((hit.point - camera.position).normalized);
        }
        else
        {
            useText.gameObject.SetActive(false);
        }
    }
}
