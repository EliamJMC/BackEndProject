using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class DictionaryManager : MonoBehaviour 
{
    public void DictionaryVerification()
    {
        string path = Path.Combine(Application.dataPath, "Data/TypesDictionary.json");
        string jsonText = File.ReadAllText(path);
        JArray typesDict = JArray.Parse(jsonText);

        foreach (JObject type in typesDict) 
        {
            string filePath = Path.Combine(Application.dataPath, "Data/" + type + "sDictionary.json");
            if (!File.Exists(filePath)) 
                File.WriteAllText(filePath, "");
        }
    }
}