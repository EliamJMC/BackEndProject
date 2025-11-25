using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> inventory = new List<Item>();
    public Item[] barItems;
    public Sprite i_con;
    public Sprite i_con2;

    string path = Path.Combine(Application.dataPath, "Data", "InfoDataSaves", "inventory.json");

    private void Start()
    {
        barItems = new Item[10];

        Load_Inv_From_JSON();

        Add_ItemToItemsBar(3, new Item(Type.RawResource)
        {
            icon = i_con,
            name = "Wood",
            description = "RawWood",
            quality = 3,
            quantity = 5,
            itemSpecifications = new RawResource
            {
                R_ID = "Wood_01",
                source = "Oak Forest",
                extracMethod = "Cutted with axe"
            }
        });

        Add_ItemToItemsBar(5, new Item(Type.RawResource)
        {
            icon = i_con2,
            name = "Rock",
            description = "RawRock",
            quality = 3,
            quantity = 5,
            itemSpecifications = new RawResource
            {
                R_ID = "Rock_01",
                source = "Oak Forest",
                extracMethod = "Cutted with axe"
            }
        });
    }

    void Add_ItemToItemsBar(int index, Item item)
    {
        barItems[index] = item;
    }

    private void OnApplicationQuit()
    {
        Save_Inv_In_JSON(inventory);
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
            Debug.Log(inventory[1].quantity + " " + inventory[i].name + "has been deleted");

        inventory.Clear();
    }

    // JSON Methods
    void Save_Inv_In_JSON(List<Item> inv)
    {
        JArray jInventory = JArray.FromObject(inv);
        var orederedInv = jInventory.OrderBy(i => (string)i["type"]).ThenBy(i => (string)i["name"]).ToList();
        jInventory = new JArray(orederedInv);

        File.WriteAllText(path, jInventory.ToString());
    }
    void Load_Inv_From_JSON() 
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("No existe archivo de inventario en: " + path);
            File.Create(path);
            return;
        }

        string jsonText = File.ReadAllText(path);

        JArray jInventory = JArray.Parse(jsonText);
        List<Item> loadedInventory = jInventory.ToObject<List<Item>>();

        inventory = loadedInventory;
    }
}


