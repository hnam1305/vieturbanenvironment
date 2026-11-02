using UnityEngine;
using System;
using System.Collections.Generic;
[System.Serializable]
public class Order
{
    public enum OrderState
    {
        Available, Making, Cooking, WaitingConfirm, Closed
    }
    [SerializeField] private RecipeData recipe;
    [SerializeField] private int requiredCount; //so cai khach dat
    [SerializeField] private int producedCount; //tong so cai da lam/me
    [SerializeField] private OrderState orderStatus;
    private List<CookingResult> results; //ket qua du/thieu/thua cua tung cai da lam
    private int orderId;
    private int unitPrice;
    private bool isCreated = false;

    public bool IsCreated
    {
        get { return isCreated; }
    }
    public int OrderId
    {
        get
        {
            return orderId;
        }
    }
    public int UnitPrice
    {
        get
        {
            return unitPrice;
        }
    }

    public RecipeData Recipe
    {
        get
        {
            return recipe;
        }
    }

    public int RequiredCount
    {
        get
        {
            return requiredCount;
        }
    }

    public int ProducedCount
    {
        get
        {
            return producedCount;
        }
    }

    public OrderState OrderStatus
    {
        get
        {
            return orderStatus;
        }
    }

    public bool IsComplete
    {
        get
        {
            return producedCount >= requiredCount;
        }
    }

    public int RemainingCount
    {
        get
        {
            return Math.Max(0, requiredCount - producedCount);
        }
    }

    public IReadOnlyList<CookingResult> Results
    {
        get
        {
            return results;
        }
    }
    public int Total
    {
        get
        {
            return requiredCount * unitPrice;
        }
    }
    public Order(RecipeData recipe, int requiredCount, int orderId, int unitPrice)
    {
        this.recipe = recipe;
        this.orderStatus = OrderState.Available;
        producedCount = 0;
        this.requiredCount = Math.Max(1, requiredCount);
        results = new List<CookingResult>();
        this.orderId = orderId;
        this.unitPrice = Math.Max(0, unitPrice);
    }

    public bool StartMaking()
    {
        if (orderStatus != OrderState.Available)
        {
            return false;
        }
        orderStatus = OrderState.Making;
        return true;

    }

    public bool MarkCooking()
    {
        if (orderStatus != OrderState.Making)
        {
            return false;
        }
        orderStatus = OrderState.Cooking;
        return true;
    }
    public bool AddCookedResults(IReadOnlyList<CookingResult> newResults)
    {
        if (orderStatus != OrderState.Cooking)
        {
            return false;
        }
        if (newResults == null || newResults.Count == 0)
        {
            return false;
        }
        results.AddRange(newResults);
        producedCount += newResults.Count;
        orderStatus = OrderState.WaitingConfirm;
        return true;
    }
    public bool StartMakeUp()
    {
        if (IsComplete == true)
        {
            return false;
        }
        if (orderStatus != OrderState.WaitingConfirm)
        {
            return false;
        }
        orderStatus = OrderState.Making;
        return true;
    }

    

    public bool Confirm()
    {
      
        if (orderStatus != OrderState.WaitingConfirm)
        {
            return false;
        }
        orderStatus = OrderState.Closed;
        return true;
    }

    public bool MarkBoxCreated()
    {
        if (orderStatus != OrderState.Closed || IsCreated == true)
        {
            return false;
        }
        isCreated = true;
        return true;
    }

}
