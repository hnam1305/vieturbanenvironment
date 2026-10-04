using System;
using System.Collections.Generic;
using System.Diagnostics;
[System.Serializable]
public class PlayerData
{
    public int money = 200000;
    public List<InventoryEntries> inventory = new();

    public void AddItem(string itemId, float amount)
    {
        foreach (var entry in inventory)
        {
            if (itemId == entry.itemId)
            {
                entry.amount += amount;
                return;
            }
            
        }
        inventory.Add(new InventoryEntries(itemId, amount));
    }
}
