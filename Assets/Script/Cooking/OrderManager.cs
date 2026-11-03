using System.Collections.Generic;
using UnityEngine;
using static Order;

public class OrderManager : MonoBehaviour
{
    [SerializeField] private CookingSession cookingSession;
    [SerializeField] private ShippingShelf shippingShelf;
    private List<Order> orders = new List<Order>();
    private Order selectedOrder;
    public IReadOnlyList<Order> Orders
    {
        get {
            return orders;
        }

    }
    public Order SelectedOrder
    {
        get { 
            return selectedOrder;
        }
    }
    //mng order lsist
    public void AddOrder(Order order)
    {
        if (order == null)
        {
            return;
        }
        foreach (Order existingOrder in orders)
        {
            if (existingOrder.OrderId == order.OrderId)
            {
                Debug.Log("Order ID already exists!");
                return;
            }
        }

        orders.Add(order);

        Debug.Log("added order: " + order.OrderId);
    }
    public Order GetOrderById(int orderId)
    {
        foreach (Order order in orders)
        {
            if (order.OrderId == orderId)
            {
                return order;
            }
        }
        return null;
    }
    public bool SelectOrder(int orderId)
    {
        Order order = GetOrderById(orderId);
        if (order == null)
        {
            Debug.Log("order not exist");
            return false;
        }
        selectedOrder = order;
        Debug.Log("selected order: " + order.OrderId);
        return true;
    }
    //acp order then connect order mng with cooking sesion
    public bool AcceptOrder(int orderId)
    {
        Order order = GetOrderById(orderId);
        if (order == null)
        {
            Debug.Log("order not exist");
            return false;
        }
        if (order.OrderStatus != OrderState.Available)
        {
            Debug.Log("not available order");
            return false;
        }
        if (cookingSession == null)
        {
            Debug.Log("cooking session not exist");
            return false;
        }
        order.StartMaking();
        bool success = cookingSession.BeginOrder(order);

        if (success == false)
        {
            Debug.Log("fail to begin session");
            return false;
        }
        selectedOrder = order;
        Debug.Log("order accept: "  + order.OrderId);
        return true;
    }
    //confirm and redo dish in case lam thieu
    public bool ConfirmSelectedOrder()
    {
        if (selectedOrder == null)
        {
            Debug.LogWarning("pls select an order");
            return false;
        }
        if (selectedOrder.OrderStatus != OrderState.WaitingConfirm)
        {
            Debug.Log("order is not in confirm state");
            return false;
        }
        selectedOrder.Confirm();
        Debug.Log("order confirmed: " + selectedOrder.OrderId);
        return true;
    }
    public bool MakeUpSelectedOrder()
    {
        if (selectedOrder == null)
        {
            Debug.Log("pls select an order");
            return false;
        }
        if (selectedOrder.IsComplete)
        {
            Debug.Log("full");
            return false;
        }
        selectedOrder.StartMakeUp();
        if (selectedOrder.StartMakeUp() == false)
        {
            return false;
        }
        bool success = cookingSession.BeginOrder(selectedOrder);
        if (success == false)
        {
            Debug.Log("fail to begin");
            return false;
        }
        Debug.Log("redoing");
        return true;
    }
    public bool CreateShippingBox()
    {
        if (selectedOrder == null)
        {
            Debug.Log("pls select an order");
            return false;
        }
        if (shippingShelf == null)
        {
            Debug.Log("pls add shipping shelf");
            return false;
        }
        if (selectedOrder.OrderStatus != OrderState.Closed)
        {
            Debug.Log("you must confirm the order");
            return false;
        }
        bool success = shippingShelf.TryCreateBox(selectedOrder);
        if (success == false)
        {
            Debug.Log("fail to create shipping box");
            return false;
        }
        Debug.Log(" created for order: " + selectedOrder.OrderId);

        return true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
