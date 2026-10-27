using UnityEngine;

public class IngredientStation : InteractableObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float capacity;
    [SerializeField] private float currentAmount;
    [SerializeField] private float portionPerClick;
    [SerializeField] private GameObject fullVisual;
    [SerializeField] private GameObject emptyVisual;
    [SerializeField] private string itemId;
    private void Start()
    {
        UpdateVisual();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Fill(capacity);
        }
    }
    public override void Interact()
    {
        Take();
        Debug.Log("con lai" + currentAmount);
    }
    public float Take()
    {
        if (currentAmount <= 0)
        {
            return 0f;
        }
        float realTaken = Mathf.Min(portionPerClick, currentAmount);
        currentAmount -= realTaken;
        UpdateVisual();
        return realTaken;
    }
    public void Fill(float amount)
    {
        float space = capacity - currentAmount;
        float haveInInventory = GameSession.Current.GetAmount(itemId);
        float toFill = Mathf.Min(amount, space, haveInInventory);
        if (toFill <= 0)
        {
            return;
        }
        if (GameSession.Current.TryRemove(itemId, toFill))
        {
            currentAmount += toFill;

        }
        UpdateVisual();
    }
    public void UpdateVisual()
    {
        fullVisual.SetActive(currentAmount > 0);
        emptyVisual.SetActive(currentAmount <= 0);
    }
}
