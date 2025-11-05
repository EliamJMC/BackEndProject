using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //Items
    public List<Item> items = new List<Item>();
    public void AddItem(Item newItem)
    {
            items.Add(newItem);
    }
    public void RemoveItem(string itemId, int quantity)
    {
        Item existingItem = items.Find(item => item.id == itemId);
        if (existingItem != null)
        {
            existingItem.quantity -= quantity;
            if (existingItem.quantity <= 0)
                items.Remove(existingItem);
        }
    }
    
    //Resources
    public List<Resource> resources = new List<Resource>();
    public void AddResource(Resource newResource) 
    { 
        resources.Add(newResource);
    }
    public void RemoveResource(string resourceId, int quantity) 
    {
        Resource existingResource = resources.Find(resources => resources.id == resourceId);
        if (existingResource != null)
        {
            existingResource.quantity -= quantity;
            if (existingResource.quantity <= 0)
                resources.Remove(existingResource);
        }
    }
}

