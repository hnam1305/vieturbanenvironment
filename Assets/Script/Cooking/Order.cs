using UnityEngine;
using System.Collections.Generic;
public class Order
{
    public enum OrderState
    {
        Available, Making, Cooking, WaitingConfirm, Closed
    }
    [SerializeField] private RecipeData recipe;
    [SerializeField] private int requiredCount;
    [SerializeField] private int producedCount;
    [SerializeField] private OrderState orderState;
    private List<CookingResult> results;
}
