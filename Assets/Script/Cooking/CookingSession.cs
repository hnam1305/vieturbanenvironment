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
    private List<GameObject> plateVisuals = new List<GameObject>();

    public bool HasIngredientsOnPlate
    {
        get { return addedAmount.Count > 0; }
    }

    public bool HasIngredientOnPlate(ItemData ingredient)
    {
        return ingredient != null && addedAmount.ContainsKey(ingredient.itemId);
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
        if (CanAccept == false || ingredient == null || amount <= 0f)
        {
            return;
        }
        if (addedAmount.ContainsKey(ingredient.itemId))
        {
            return;
        }
        addedAmount[ingredient.itemId] = amount;
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
        Transform visualRoot = FindIngredientVisualRoot(ingredient);
        if (visualRoot == null)
        {
            Debug.Log("dish point is missing");
            return;
        }

        GameObject visual = Instantiate(ingredient.worldPrefab, visualRoot.position, visualRoot.rotation);
        // Set the world scale before parenting so a scaled DishPoint hierarchy
        // cannot enlarge the ingredient unexpectedly.
        visual.transform.localScale = ingredient.worldPrefab.transform.localScale * plateVisualScale;
        visual.transform.SetParent(visualRoot, true);
        if (visualRoot == plateVisualRoot)
        {
            visual.transform.position += Vector3.right * plateVisuals.Count * 0.2f;
        }
        plateVisuals.Add(visual);
    }

    private Transform FindIngredientVisualRoot(ItemData ingredient)
    {
        if (ingredient == null)
        {
            return plateVisualRoot;
        }

        string itemName = NormalizeAnchorName(ingredient.itemName);
        if (itemName.Length > 0)
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child == transform || child == plateVisualRoot)
                {
                    continue;
                }

                string childName = NormalizeAnchorName(child.name);
                if (childName == "dishpoint" + itemName || childName == itemName + "dishpoint")
                {
                    return child;
                }
            }
        }

        return plateVisualRoot;
    }

    private static string NormalizeAnchorName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        System.Text.StringBuilder normalized = new System.Text.StringBuilder(value.Length);
        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                normalized.Append(char.ToLowerInvariant(character));
            }
        }
        return normalized.ToString();
    }

    private void ClearPlateVisuals()
    {
        foreach (GameObject visual in plateVisuals)
        {
            if (visual != null)
            {
                Destroy(visual);
            }
        }
        plateVisuals.Clear();
    }
    public CookingResult Evaluate()
    {
        if (currentRecipe == null)
        {

            Debug.Log("missing current recipe");
            return new CookingResult();
        }
        CookingResult result = new CookingResult();
        HashSet<string> recipeIds = new HashSet<string>();
        foreach (RecipeIngredient need in currentRecipe.Ingredients)
        {
            if (need == null || need.ingredient == null)
            {
                continue;
            }

            recipeIds.Add(need.ingredient.itemId);
        }

        foreach (string ingredientId in recipeIds)
        {
            if (!addedAmount.TryGetValue(ingredientId, out float have) || have <= 0f)
            {
                result.ingredientMissingCount++;
            }
        }
        foreach (var pair in addedAmount)
        {
            if (!recipeIds.Contains(pair.Key))
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
        foreach (GameObject visual in plateVisuals)
        {
            if (visual != null)
            {
                oven.StageDishVisual(currentBatch, visual);
            }
        }
        plateVisuals.Clear();
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
