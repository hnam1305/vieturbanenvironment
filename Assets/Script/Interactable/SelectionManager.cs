using UnityEngine;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public GameObject interaction_Info_UI;
    private TextMeshProUGUI interaction_text;

    private void Start()
    {
        interaction_text = interaction_Info_UI.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //camera.main: property built in tra ve main camera
                                                                    //input.mouseposition: vi tri chuot tren man hinh, dang vector 3
                                                                    //Camera.main.ScreenPointToRay: ham chuyen doi 1 diem 2d tren man hinh thanh 1 ray trong 3d, bat dau tu
                                                                    //camera  va xuyen qua dung diem do tren man hinh
                                                                   
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) //ham Physics.Raycast ban ray ra va tra ve true/false, neu true thi ghi thong tin va cham vao hit.
        {
            InteractableObject interactable = hit.transform.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interaction_text.text = interactable.GetItemName();
                interaction_Info_UI.SetActive(true);
            }
            else
            {
                interaction_Info_UI.SetActive(false);
            }

        }
        else
        {
            interaction_Info_UI.SetActive(false);
        }

    }
}
        
 
