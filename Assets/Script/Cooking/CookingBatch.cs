using UnityEngine;
using System;
using System.Collections.Generic;
public enum BatchState
{
    Staging, Cooking, Done
}
[System.Serializable]
public class CookingBatch
{
    [SerializeField] private Order order;
    private List<CookingResult> results;
    [SerializeField] private BatchState state;
    [SerializeField] private float elapsed;
    [SerializeField] private float cookTime;

    public IReadOnlyList<CookingResult> Results
    {
        get
        {
            return results;
        }
    }
    public BatchState State
    {
        get
        {
            return state;
        }
    }
    public int StagedCount
    {
        get
        {
            return results.Count;
        }
    }
    public bool IsDone
    {
        get
        {
            return (state == BatchState.Done);
        }
    }
    public float Progress
    {
        get
        {
            return Mathf.Clamp01(elapsed / cookTime);
        }
    }

    public CookingBatch(Order order)
    {
        
        this.order = order;
        if (order == null || order.Recipe == null)
        {
            cookTime = 0;
            Debug.Log("need order");
        }
        cookTime = Math.Max(0.01f, order.Recipe.CookTime);
        results = new List<CookingResult>();
        state = BatchState.Staging;
        elapsed = 0f;
        
    }
}
