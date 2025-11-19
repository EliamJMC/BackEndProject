using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> inventory = new List<Item>();
    private void Start()
    {
        Add_ItemToInv(new Item(Type.RawResource)
        {
            name = "Wood",
            description = "RawWood",
            properties = new string[] { "inflamable", "hard", "flexible" },
            quality = 3,
            quantity = 5,
            itemSpecifications = new RawResource
            {
                R_ID = "Wood_01",
                source = "Oak Forest",
                extracMethod = "Cutted with axe"
            }

        });

    }

    void Add_ItemToInv(Item itemToAdd)
    {
        for (int i = 0; i < inventory.Count; i++) {
            if (inventory[i].name == itemToAdd.name && inventory[i].quality == itemToAdd.quality) {
                inventory[i].quantity += itemToAdd.quantity;
                if (inventory[i].quantity > inventory[i].maxStack)
                    inventory[i].quantity = inventory[i].maxStack;
                return;
            }
        }
        inventory.Add(itemToAdd);
    }

    void Remove_ItemFromInv(Item itemToRemove)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].name == itemToRemove.name && inventory[i].quality == itemToRemove.quality)
            {
                inventory[i].quantity -= itemToRemove.quantity;
                if (inventory[i].quantity <= 0)
                    inventory.Remove(itemToRemove);
                return;
            }
        }
    }

    void Clear_Inv()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            Debug.Log(inventory[1].quantity + " " + inventory[i].name + "has been deleted");
        }
        inventory.Clear();
    }

    void Save_Inv_JSON(List<Item> inv)
    {
        string basePath = Path.Combine(Application.dataPath, "InfoDataSaves" );
        string jsonFilePath = "inventory.json";
        string jsonToAdd = "";
        JArray jInventory = JArray.FromObject(inventory);

        var placeBy = jInventory.OrderBy(i => (string)i["type"]).ThenBy(i => (string)i["name"]).ToList();
        jInventory = new JArray(placeBy);

        File.WriteAllText(basePath + jsonFilePath, jInventory.ToString);

    }
}


