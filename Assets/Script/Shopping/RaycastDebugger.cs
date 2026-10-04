using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RaycastDebugger : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        var data = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count == 0)
        {
            Debug.Log("[Raycast] Không trúng UI nào");
            return;
        }

        for (int i = 0; i < results.Count; i++)
        {
            Transform t = results[i].gameObject.transform;
            string path = t.name;
            while (t.parent != null && path.Split('/').Length < 5)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            Debug.Log($"[Raycast] #{i}: {path}");
        }
    }
}