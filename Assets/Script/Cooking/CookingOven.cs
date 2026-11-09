using UnityEngine;
using System.Collections.Generic;
//holding batch list, each frame call Tick for each batch, when a batch is completed, call order.Addcookedresults to create shipbox then delete that batch from the list
//and also send 2 events: progress per bacth for UI and the completed batch
public class CookingOven : InteractableObject
{
   
    private List<CookingBatch> cookingBatches = new List<CookingBatch>();
    public event System.Action<CookingBatch> OnBatchProgress; //update progress on UI
    public event System.Action<CookingBatch> OnBatchDone;
    private CookingSession cookingSession;

    public void SetCookingSession(CookingSession session)
    {
        cookingSession = session;
    }

    public override string GetInteractionPrompt()
    {
        if (cookingSession != null && cookingSession.HasStagedDish)
        {
            return "press k to cook";
        }
        return "oven";
    }

    public override bool HandleInteractionKey(KeyCode key)
    {
        if (key == KeyCode.K && cookingSession != null)
        {
            return cookingSession.StartCookingBatch();
        }
        return false;
    }
    

    void Update()
    {
    for (int i = cookingBatches.Count - 1; i >= 0; i--)
        {
            CookingBatch batch = cookingBatches[i];
            bool done = batch.Tick(Time.deltaTime);
            if (OnBatchProgress != null)
            {
                OnBatchProgress(batch);
            }
            if (done == true)
            {
                bool result = batch.Order.AddCookedResults(batch.Results);
                if (result == false)
                {
                    Debug.Log("addcookedresults fail");
                }
                cookingBatches.RemoveAt(i);
                if (OnBatchDone != null)
                {
                    OnBatchDone(batch);
                    
                }
            }
        }  
        
        
    }
    public bool StartBatch(CookingBatch batch)
    {
        if (batch == null)
        {
            return false;
        }
        if (batch.State != BatchState.Staging)
        { return false; }
        if (batch.StagedCount <= 0)
        {
            return false;
        }
        if (batch.Order == null)
        {
            return false;
        }
        if (batch.Order.OrderStatus != Order.OrderState.Making)
        {
            return false;
        }
        batch.StartCooking();
        batch.Order.MarkCooking();
        cookingBatches.Add(batch);
        return true;
    }


}
