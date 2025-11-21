using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Item;

public class ItemsPrefDataDefiner : MonoBehaviour
{
    public Type ItemDataFile;

    public List<Item> Items = new List<Item>();

    private void Awake()
    {
        Load_From_JSON();
    }
    private void OnApplicationQuit()
    {
        Save_In_JSON(Items);
    }

    void Save_In_JSON(List<Item> items)
    {
        string path = Path.Combine(Application.dataPath, "Data", "BaseItemDataDef", ItemDataFile.ToString() + ".json");
        JArray j_Items = JArray.FromObject(items);
        var orederedInv = j_Items.OrderBy(i => (string)i["name"]).ToList();
        j_Items = new JArray(orederedInv);

        File.WriteAllText(path, j_Items.ToString());
    }

    void Load_From_JSON()
    {
        string path = Path.Combine(Application.dataPath, "Data", "BaseItemDataDef", ItemDataFile.ToString() + ".json");
        string jsonText = File.ReadAllText(path);

        JArray j_Items = JArray.Parse(jsonText);
        List<Item> loadedInventory = j_Items.ToObject<List<Item>>();

        Items = loadedInventory;
    }
}
