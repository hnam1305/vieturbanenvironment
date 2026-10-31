using System.Collections.Generic;
using UnityEngine;
using System.Collections;
[System.Serializable]
public class CookingResult
{
    public int ingredientMissingCount;
    public int ingredientExtraCount;
    public bool isCorrect;


}
public class CookingSession : MonoBehaviour
{
    [SerializeField] private RecipeData currentRecipe;
    [SerializeField] private Transform dishPoint;
    public event System.Action<float> OnProgress;
    public event System.Action<RecipeData, CookingResult> OnDishCooked;
    private bool isCooking;
    private GameObject spawnedDish;
    private Dictionary<string, float> addedAmount = new Dictionary<string, float>();
    public bool CanAccept
    {
        get
        {
            return !isCooking;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CookingResult result = Evaluate();

            Debug.Log($"thieu {result.ingredientMissingCount},  thua {result.ingredientExtraCount} + ket qua: {result.isCorrect}");
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCooking();
        }
    }
    public void AddedIngredient(ItemData ingredient, float amount)
    {
        if (isCooking)
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
        foreach (var pair in addedAmount)
        {
            Debug.Log(pair.Key + ": " + pair.Value);
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
    public void StartCooking()
    {
        if (isCooking == true || currentRecipe == null)
        {
            return;
        }
        if (dishPoint == null)
        {
            Debug.Log("missing dish point!");
            return;
        }
        if (currentRecipe.IncompletedDish == null || currentRecipe.CompletedDish == null)
        {
            Debug.Log("missing dish");
            return;
        }
        if (addedAmount.Count == 0)
        {
            return;
        }
        if (spawnedDish != null)
        {
            Destroy(spawnedDish);
        }

        
        CookingResult result = Evaluate();
        isCooking = true;
        StartCoroutine(CookRoutine(result));
    }
    private IEnumerator CookRoutine(CookingResult result)
    {
        RecipeData recipe = currentRecipe;
        float time = 0f;
        while (time < recipe.CookTime)
        {
            time += Time.deltaTime;
            if (OnProgress != null)
            {
                OnProgress(Mathf.Clamp01(time / recipe.CookTime));
            }
            yield return null;
        }
        if (OnProgress != null)
        {
            OnProgress(1f);
        }
        GameObject prefabs;
        if (result.isCorrect)
        {
            prefabs = recipe.CompletedDish;
        }
        else
        {
            prefabs = recipe.IncompletedDish;
        }
        if (prefabs != null && dishPoint != null)
        {
            spawnedDish = Instantiate(prefabs, dishPoint.position, Quaternion.identity);

        }
        else
        {
            Debug.Log("can not find prefabs");
        }
        
        addedAmount.Clear();
        isCooking = false;
        if (OnDishCooked != null)
        {
            OnDishCooked(recipe, result);
        }
    } 

        
}
