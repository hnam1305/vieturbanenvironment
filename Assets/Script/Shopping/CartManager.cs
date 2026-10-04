using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CartEntry
{
    public ItemData item;
    public float amount; 

    public float LineTotal => item.basePriceBuy * amount;
}


public class CartManager : MonoBehaviour
{
    [Header("Túi đồ (sức chứa)")]
    [SerializeField] private float bagCapacity = 100f;
    [SerializeField] private Slider bagFillSlider;
    [SerializeField] private TMP_Text bagPercentText;

    [Header("Danh sách giỏ hàng")]
    [SerializeField] private Transform cartContentParent;
    [SerializeField] private CartItemRowUI cartRowPrefab;

    [Header("Tổng kết")]
    [SerializeField] private TMP_Text playerMoneyText;
    [SerializeField] private TMP_Text totalCostText;
    [SerializeField] private Button checkoutButton;
    [SerializeField] private PlayerWallet playerWallet;
    private readonly List<CartEntry> _cart = new();
    private readonly List<CartItemRowUI> _spawnedRows = new();
    private float _currentLoad;

    public event Action<CartEntry> OnItemAdded;
    public event Action OnCheckout;

    private void Start()
    {
        checkoutButton.onClick.AddListener(HandleCheckout);
        RefreshUI();
        playerWallet.MoneyChange += HandleMoneyChanged;
    }
    private void OnDestroy()
    {
        if (playerWallet != null)
            playerWallet.MoneyChange -= HandleMoneyChanged;
    }

    public bool TryAddItem(ItemData item, float amount)
    {
        float addedLoad = amount;

        if (_currentLoad + addedLoad > bagCapacity)
        {
            Debug.Log("Bag is full, cannot add more");
            return false;
        }

        CartEntry existing = _cart.Find(e => e.item == item);
        if (existing != null)
            existing.amount += amount;
        else
            _cart.Add(new CartEntry { item = item, amount = amount });

        _currentLoad += addedLoad;
        RefreshUI();
        OnItemAdded?.Invoke(existing ?? _cart[^1]);
        return true;
    }

    public void RemoveItem(CartEntry entry)
    {
        _currentLoad -= entry.amount;
        _cart.Remove(entry);
        RefreshUI();
    }

    private void HandleCheckout()
    {
        if (_cart.Count == 0)
        {
            return;
        }
        int total = GetCartTotal();
        bool result = playerWallet.TrySpend(total);
        if (result == false)
        {
            return;
        }
        foreach (CartEntry entry in _cart)
        {
            playerWallet.AddItem(entry.item.itemId, entry.amount);
        }
        _cart.Clear();
        _currentLoad = 0;
        RefreshUI();
        OnCheckout?.Invoke();
    }

    private int GetCartTotal()
    {
        float total = 0f;
        foreach (CartEntry entry in _cart)
            total += entry.LineTotal;
        return Mathf.RoundToInt(total);
    }

    private void RefreshUI()
    {
        float fillRatio = Mathf.Clamp01(_currentLoad / bagCapacity);
        bagFillSlider.value = fillRatio;
        bagPercentText.text = $"{fillRatio * 100f:0}%";

        foreach (CartItemRowUI row in _spawnedRows)
            Destroy(row.gameObject);
        _spawnedRows.Clear();

        int total = GetCartTotal();
        foreach (CartEntry entry in _cart)
        {
            CartItemRowUI row = Instantiate(cartRowPrefab, cartContentParent);
            row.Setup(entry, () => RemoveItem(entry));
            _spawnedRows.Add(row);
        }

        playerMoneyText.text = $"Current cash: {playerWallet.Money:N0}đ";
        totalCostText.text = $"Total payment: {total:N0}đ";
        checkoutButton.interactable = _cart.Count > 0 && total <= playerWallet.Money;
    }
    private void HandleMoneyChanged(int newMoney)
    {
        RefreshUI();
    }
}