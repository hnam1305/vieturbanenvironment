using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CookingResult
{
    public int ingredientMissingCount;
    public int ingredientExtraCount;
    public bool isCorrect;


}
//hold the ingredient then evaluate it, assign the result to current batch
public class CookingSession : InteractableObject
{
    private RecipeData currentRecipe;
    private CookingBatch currentBatch;
    [SerializeField] CookingOven oven;
    [SerializeField] private Transform plateVisualRoot;
    [SerializeField] private float plateVisualScale = 0.5f;
    private Dictionary<string, float> addedAmount = new Dictionary<string, float>();
    private GameObject plateVisual;

    public bool HasIngredientsOnPlate
    {
        get { return addedAmount.Count > 0; }
    }

    public bool HasStagedDish
    {
        get { return currentBatch != null && currentBatch.StagedCount > 0; }
    }

    public override string GetInteractionPrompt()
    {
        if (HasIngredientsOnPlate)
        {
            return "press j to add to batch";
        }
        return "plate";
    }

    public override bool HandleInteractionKey(KeyCode key)
    {
        if (key == KeyCode.J)
        {
            return StageDish();
        }
        return false;
    }

    public override void Interact()
    {
    }

    private void Awake()
    {
        if (plateVisualRoot == null)
        {
            plateVisualRoot = transform.Find("DishPoint");
        }
        if (oven != null)
        {
            oven.SetCookingSession(this);
        }
    }
    public bool CanAccept
    {
        get
        {
            if (currentBatch != null && currentBatch.State == BatchState.Staging)
            {
                return true;
            }
            return false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            StageDish();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCookingBatch();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            CookingResult result = Evaluate();

            Debug.Log($"thieu {result.ingredientMissingCount},  thua {result.ingredientExtraCount} + ket qua: {result.isCorrect}");
        }
    }
    public void AddedIngredient(ItemData ingredient, float amount)
    {
        if (CanAccept == false)
        {
            return;
        }
        if (addedAmount.TryGetValue(ingredient.itemId, out float currentAmount))
        {
            addedAmount[ingredient.itemId] = currentAmount + amount;
        }
        else
        {
            addedAmount[ingredient.itemId] = amount;
        }
        AddPlateVisual(ingredient);
        foreach (var pair in addedAmount)
        {
            Debug.Log(pair.Key + ": " + pair.Value);
        }
    }

    private void AddPlateVisual(ItemData ingredient)
    {
        if (plateVisualRoot == null)
        {
            Debug.Log("plate visual root is missing");
            return;
        }
        if (ingredient == null || ingredient.worldPrefab == null)
        {
            Debug.Log("ingredient world prefab is missing");
            return;
        }
        if (plateVisual == null)
        {
            plateVisual = Instantiate(ingredient.worldPrefab, plateVisualRoot.position, plateVisualRoot.rotation);
            plateVisual.transform.localScale = ingredient.worldPrefab.transform.localScale * plateVisualScale;
            Debug.Log("plate visual spawned");
        }
    }

    private void ClearPlateVisuals()
    {
        if (plateVisual != null)
        {
            Destroy(plateVisual);
            plateVisual = null;
        }
    }
    public CookingResult Evaluate()
    {
        if (currentRecipe == null)
        {

            Debug.Log("missing current recipe");
            return new CookingResult();
        }
        CookingResult result = new CookingResult();
        HashSet<string> recipeId = new HashSet<string>();
        foreach (RecipeIngredient need in currentRecipe.Ingredients)
        {
            addedAmount.TryGetValue(need.ingredient.itemId, out float have);
            recipeId.Add(need.ingredient.itemId);
            if (have < need.amount)
            {
                result.ingredientMissingCount++;
            }
            if (have > need.amount)
            {
                result.ingredientExtraCount++;
            }

        }
        foreach (var pair in addedAmount)
        {
            if (!recipeId.Contains(pair.Key))
            {
                result.ingredientExtraCount++;
            }
        }
        if (result.ingredientMissingCount == 0 && result.ingredientExtraCount == 0)
        {
            result.isCorrect = true;
        }
        return result;
    }
    public bool BeginOrder(Order order) //start making an order
    {
        if (order == null || order.OrderStatus != Order.OrderState.Making || order.Recipe == null)
        {
            return false;
        }
        // Keep one order on the plate at a time. Once its batch is sent to
        // the oven, currentBatch becomes null and another order can begin.
        if (addedAmount.Count > 0 || currentBatch != null)
        {
            return false;
        }
        currentRecipe = order.Recipe;
        currentBatch = new CookingBatch(order);
        return true;
    }
    public bool StageDish() {
        if (currentBatch == null || CanAccept == false || addedAmount.Count == 0)
        {
            Debug.Log("no active dish to add to batch");
            return false;
        }
        CookingResult result = Evaluate();
        if (currentBatch.Stage(result) == false)
        {
            Debug.Log("batch rejected dish");
            return false;
        }
        addedAmount.Clear();
        Debug.Log("dish added to batch");
        return true;
    } //assign the dish which is just maked to the oven

    public bool StartCookingBatch()
    {
        if (currentBatch == null || oven == null)
        {
            Debug.Log("no staged batch or oven");
            return false;
        }
        if (oven.StartBatch(currentBatch) == false)
        {
            Debug.Log("oven rejected batch");
            return false;
        }
        currentBatch = null;
        ClearPlateVisuals();
        Debug.Log("cooking started");
        return true;
    }
}
