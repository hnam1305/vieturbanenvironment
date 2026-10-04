using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CartItemRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameQuantityText; 
    [SerializeField] private TMP_Text lineTotalText;  
    [SerializeField] private Button removeButton;       

    public void Setup(CartEntry entry, Action onRemove)
    {
        string unit = entry.item.sellType == SellType.Gram ? "g" : "";
        nameQuantityText.text = $"{entry.item.itemName} x {entry.amount:0}{unit}";
        lineTotalText.text = $"Subtotal: {entry.LineTotal:N0}";

        removeButton.onClick.RemoveAllListeners();
        removeButton.onClick.AddListener(() => onRemove?.Invoke());
    }
}