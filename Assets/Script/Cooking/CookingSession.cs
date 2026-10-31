using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Unity.Mathematics;
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
    private bool isCooking;
    private Dictionary<string, float> addedAmount = new Dictionary<string, float>();

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
        if (addedAmount.Count == 0)
        {
            return;
        }
        CookingResult result = Evaluate();
        isCooking = true;
        StartCoroutine(CookRoutine(result));
    }
    private IEnumerator CookRoutine(CookingResult result)
    {
        float time = 0f;
        while (time < currentRecipe.CookTime)
        {
            time += Time.deltaTime;
            yield return null;
        }
        GameObject prefabs;
        if (result.isCorrect)
        {
            prefabs = currentRecipe.CompletedDish;
        }
        else
        {
            prefabs = currentRecipe.IncompletedDish;
        }
        Instantiate(prefabs, dishPoint.position, quaternion.identity);
        addedAmount.Clear();
        isCooking = false;
    } 

        
}
