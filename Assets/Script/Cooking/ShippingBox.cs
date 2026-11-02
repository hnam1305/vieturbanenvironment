using UnityEngine;

public class ShippingBox : InteractableObject
{
    private Order order;
    public Order Order
    {
        get
        {
            return order;
        }
    }
    public int OrderId
    {
        get
        {
            if( order == null)
            {
                return 0;
            }
            return order.OrderId;
        }
    }
    public int DishCount
    {
        get
        {
            if (order == null)
            {
                return 0;
            }
            return order.ProducedCount;
        }
    }
    public bool SetUp(Order newOrder)
    {
        if (newOrder == null)
        {
            return false;
        }
        if (order != null)
        {
            return false;
        }
        if (newOrder.OrderStatus != Order.OrderState.Closed)
        {
            return false;
        }
        if (newOrder.Recipe == null)
        {
            return false;
        }
        order = newOrder;
        
        ItemName = "Order #" + newOrder.OrderId + ": " + newOrder.Recipe.RecipeName;
        return true;
    }
    public override void Interact()
    {
        if (order == null)
        {
            Debug.Log("box has no order");
            return;
        }

        Debug.Log("Order ID: " +  order.OrderId + ": " + order.Recipe.RecipeName + " | quantity: " + order.ProducedCount);
    }
}
