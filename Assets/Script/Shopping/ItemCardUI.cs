using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ItemCardUI : MonoBehaviour
{
    [Header("Thông tin chung")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Chọn số lượng - dạng CÁI (SellType.Quantity)")]
    [SerializeField] private GameObject quantityGroup;
    [SerializeField] private TMP_Text quantityValueText;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;

    [Header("Chọn số lượng - dạng GRAM (SellType.Gram)")]
    [SerializeField] private GameObject sliderGroup;
    [SerializeField] private Slider gramSlider;
    [SerializeField] private TMP_Text gramValueText;

    [Header("Hành động")]
    [SerializeField] private Button addButton;
    public event Action<ItemData, float> OnAddToCart;

    private ItemData _data;
    private float _currentAmount;

    private void Awake()
    {
        decreaseButton.onClick.AddListener(() => ChangeQuantity(-1));
        increaseButton.onClick.AddListener(() => ChangeQuantity(1));
        gramSlider.onValueChanged.AddListener(OnSliderChanged);
        addButton.onClick.AddListener(HandleAddClicked);
    }

    public void Setup(ItemData data)
    {
        _data = data;
        nameText.text = data.itemName;
        descriptionText.text = data.description;
        iconImage.sprite = data.icon;

        bool isGram = data.sellType == SellType.Gram;
        sliderGroup.SetActive(isGram);
        quantityGroup.SetActive(!isGram);
        _currentAmount = 0;

        if (isGram)
        {
            gramSlider.minValue = 0;
            gramSlider.maxValue = Mathf.Max(1, data.limitPerBuying);
            gramSlider.SetValueWithoutNotify(0);
            UpdateGramDisplay();
            priceText.text = $"Price: {data.basePriceBuy:N0}/gram";
        }
        else
        {
            quantityValueText.text = "0";
            priceText.text = $"Price: {data.basePriceBuy:N0}/piece";
        }
    }

    private void ChangeQuantity(int delta)
    {
        float max = _data.limitPerBuying > 0 ? _data.limitPerBuying : float.MaxValue;
        _currentAmount = Mathf.Clamp(_currentAmount + delta, 0, max);
        quantityValueText.text = _currentAmount.ToString("0");
    }

    private void OnSliderChanged(float value)
    {
        _currentAmount = value;
        UpdateGramDisplay();
    }

    private void UpdateGramDisplay()
    {
        gramValueText.text = $"{_currentAmount:0}g";
    }

    private void HandleAddClicked()
    {
        if (_currentAmount <= 0 || _data == null) return;

        OnAddToCart?.Invoke(_data, _currentAmount);
        _currentAmount = 0;
        if (_data.sellType == SellType.Gram)
            gramSlider.SetValueWithoutNotify(0);
        else
            quantityValueText.text = "0";

        UpdateGramDisplay();
    }
}