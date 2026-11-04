using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OrderCustomerProfile
{
    public Transform customerNpc;
    public string customerName;
    public string deliveryAddress;
}

public class OrderSpawner : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private TimeController timeController;
    [SerializeField] private List<OrderCustomerProfile> customers = new List<OrderCustomerProfile>();
    [SerializeField] private List<RecipeData> recipes = new List<RecipeData>();
    [SerializeField] private int firstOrderId = 1;
    [SerializeField] private int minRequiredCount = 1;
    [SerializeField] private int maxRequiredCount = 3;
    [SerializeField] private int unitPrice = 10000;
    [SerializeField] private float firstOrderDelayGameMinutes;
    [SerializeField] private float minSpawnIntervalGameMinutes = 30f;
    [SerializeField] private float maxSpawnIntervalGameMinutes = 60f;
    [SerializeField] private float expectedDeliveryGameMinutes = 120f;

    private DateTime nextSpawnTime;
    private int nextOrderId;
    private bool scheduleInitialized;

    private void Awake()
    {
        nextOrderId = Mathf.Max(1, firstOrderId);
    }

    private void Update()
    {
        if (orderManager == null || timeController == null)
        {
            return;
        }

        DateTime now = timeController.CurrentTime;
        if (now == DateTime.MinValue)
        {
            return;
        }

        if (!scheduleInitialized)
        {
            nextSpawnTime = now.AddMinutes(Mathf.Max(0f, firstOrderDelayGameMinutes));
            scheduleInitialized = true;
        }

        if (now < nextSpawnTime)
        {
            return;
        }

        TrySpawnOrder(now);
        float minInterval = Mathf.Max(0f, Mathf.Min(minSpawnIntervalGameMinutes, maxSpawnIntervalGameMinutes));
        float maxInterval = Mathf.Max(minInterval, Mathf.Max(minSpawnIntervalGameMinutes, maxSpawnIntervalGameMinutes));
        nextSpawnTime = now.AddMinutes(UnityEngine.Random.Range(minInterval, maxInterval));
    }

    private bool TrySpawnOrder(DateTime placedAt)
    {
        List<OrderCustomerProfile> validCustomers = new List<OrderCustomerProfile>();
        foreach (OrderCustomerProfile customer in customers)
        {
            if (customer != null && customer.customerNpc != null)
            {
                validCustomers.Add(customer);
            }
        }

        List<RecipeData> validRecipes = new List<RecipeData>();
        foreach (RecipeData recipe in recipes)
        {
            if (recipe != null)
            {
                validRecipes.Add(recipe);
            }
        }

        if (validCustomers.Count == 0 || validRecipes.Count == 0)
        {
            Debug.LogWarning("OrderSpawner needs at least one customer NPC and one recipe.", this);
            return false;
        }

        OrderCustomerProfile selectedCustomer = validCustomers[UnityEngine.Random.Range(0, validCustomers.Count)];
        RecipeData selectedRecipe = validRecipes[UnityEngine.Random.Range(0, validRecipes.Count)];
        int minCount = Mathf.Max(1, Mathf.Min(minRequiredCount, maxRequiredCount));
        int maxCount = Mathf.Max(minCount, Mathf.Max(minRequiredCount, maxRequiredCount));
        int requiredCount = UnityEngine.Random.Range(minCount, maxCount + 1);
        DateTime expectedDeliveryAt = placedAt.AddMinutes(Mathf.Max(0f, expectedDeliveryGameMinutes));

        Order order = new Order(
            selectedRecipe,
            requiredCount,
            nextOrderId++,
            Mathf.Max(0, unitPrice),
            selectedCustomer.customerNpc,
            selectedCustomer.customerName,
            selectedCustomer.deliveryAddress,
            placedAt,
            expectedDeliveryAt);

        orderManager.AddOrder(order);
        return true;
    }
}
