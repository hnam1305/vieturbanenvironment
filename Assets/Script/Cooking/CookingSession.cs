using System.Collections.Generic;
using UnityEngine;
public class CookingSession : MonoBehaviour
{
    [SerializeField] private RecipeData currentRecipe;
    private Dictionary<string, float> addedAmount = new Dictionary<string, float>();

 
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
            Debug.Log(pair.Key + pair.Value);
        }
    }
}
