using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<string, int> items = new Dictionary<string, int>();


    private void Awake()
    {
        AddItem("Key");
    }

    public bool HasItem(string itemName)
    {
        return items.ContainsKey(itemName) && items[itemName] > 0;
    }

    public void AddItem(string itemName, int quantity = 1)
    {
        if (items.ContainsKey(itemName))
        {
            items[itemName] += quantity;
        }
        else
        {
            items[itemName] = quantity;
        }

        Debug.Log($"Added {quantity} {itemName}(s). Total: {items[itemName]}");
    }

    public bool RemoveItem(string itemName, int quantity = 1)
    {
        if (HasItem(itemName) && items[itemName] >= quantity)
        {
            items[itemName] -= quantity;
            return true;
        }
        return false;
    }

    public int GetItemCount(string itemName)
    {
        return items.ContainsKey(itemName) ? items[itemName] : 0;
    }
}

