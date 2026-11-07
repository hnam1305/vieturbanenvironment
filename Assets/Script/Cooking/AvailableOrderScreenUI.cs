using System.Collections.Generic;
using UnityEngine;

public class AvailableOrderScreenUI : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private OrderCardView orderCardPrefab;
    [SerializeField] private Transform orderCardContainer;
    [SerializeField, Min(0.1f)] private float refreshInterval = 0.5f;

    private readonly List<int> displayedOrderIds = new List<int>();
    private float refreshTimer;

    private void OnEnable()
    {
        RefreshOrders();
    }

    private void Update()
    {
        refreshTimer += Time.unscaledDeltaTime;
        if (refreshTimer < refreshInterval)
        {
            return;
        }

        refreshTimer = 0f;
        RefreshOrders();
    }

    public void RefreshOrders()
    {
        if (orderManager == null || orderCardPrefab == null || orderCardContainer == null)
        {
            return;
        }

        List<Order> availableOrders = new List<Order>();
        foreach (Order order in orderManager.Orders)
        {
            if (order != null && order.OrderStatus == Order.OrderState.Available)
            {
                availableOrders.Add(order);
            }
        }

        if (DisplayedOrdersMatch(availableOrders))
        {
            return;
        }

        ClearGeneratedCards();
        displayedOrderIds.Clear();

        foreach (Order order in availableOrders)
        {
            OrderCardView card = Instantiate(orderCardPrefab, orderCardContainer);
            int orderId = order.OrderId;
            card.SetOrder(order, delegate { AcceptOrder(orderId); });
            displayedOrderIds.Add(orderId);
        }
    }

    private bool DisplayedOrdersMatch(List<Order> availableOrders)
    {
        if (displayedOrderIds.Count != availableOrders.Count)
        {
            return false;
        }

        for (int index = 0; index < availableOrders.Count; index++)
        {
            if (displayedOrderIds[index] != availableOrders[index].OrderId)
            {
                return false;
            }
        }

        return true;
    }

    private void ClearGeneratedCards()
    {
        for (int index = orderCardContainer.childCount - 1; index >= 0; index--)
        {
            Destroy(orderCardContainer.GetChild(index).gameObject);
        }
    }

    private void AcceptOrder(int orderId)
    {
        if (orderManager.AcceptOrder(orderId))
        {
            RefreshOrders();
        }
    }
}
