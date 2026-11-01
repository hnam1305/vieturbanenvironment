using UnityEngine;
using System.Collections.Generic;
public class OrderDebug : MonoBehaviour
{
     private Order order;
    [SerializeField] private RecipeData recipeData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) {
            order = new Order(recipeData, 3);
            Log("StartMaking", order.StartMaking());
            Log("Cooking", order.MarkCooking());
            List<CookingResult> resultList = new List<CookingResult>();
            resultList.Add(new CookingResult());
            resultList.Add(new CookingResult());
            Log("Cooked", order.AddCookedResults(resultList));

            Log("StartMakeUp", order.StartMakeUp());
            Log("Cooking", order.MarkCooking());
            resultList = new List<CookingResult>();
            resultList.Add(new CookingResult());
            resultList.Add(new CookingResult());
            resultList.Add(new CookingResult());
            Log("Cooked", order.AddCookedResults(resultList));
            Log("Confirm", order.Confirm());
            Log("Confirm", order.Confirm());
            Log("Cooked", order.AddCookedResults(null));
        }

    }
    private void Log(string label, bool status)
    {
        Debug.Log(label + " -" + status + " | " + order.OrderStatus + " | produced " + order.ProducedCount + " | results " + order.Results.Count + " | complete " + order.IsComplete);


    }
}
