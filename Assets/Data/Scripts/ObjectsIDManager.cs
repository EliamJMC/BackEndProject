using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ObjectIDManager
{
	public const string base62 = "0123456789abcdefghijklmopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    DictionaryManager dictManager = new DictionaryManager();

    public string DefineID(string name, char cType)
	{
		DateTime startTime = DateTime.Now;
		string id = "";
		int[] iTime = { startTime.Second, startTime.Minute, startTime.Hour };
		int[] iDate = { startTime.Day, startTime.Month, startTime.Year - 2000 };

		if (name.Length > 6)
		{
			string
				begining = name.Substring(0, 3),
				ending = name.Substring(name.Length - 3);

			foreach (char c in begining)
			{
				int value = (int)c;
				if (value >= 'A' && value <= 'Z')
					value -= ('A' - 1);
				else if (value >= 'a' && value <= 'z')
					value -= ('a' - 1);

				id += base62[value];
			}

			foreach (char c in ending)
			{
				int value = (int)c;
				if (value >= 'A' && value <= 'Z')
					value -= ('A' - 1);
				else if (value >= 'a' && value <= 'z')
					value -= ('a' - 1);

				id += base62[value];
			}
		}
		else if (name.Length <= 6)
		{
			foreach (char c in name)
			{
				int value = (int)c;
				if (value >= 'A' && value <= 'Z')
					value -= ('A' - 1);
				else if (value >= 'a' && value <= 'z')
					value -= ('a' - 1);

				id += base62[value];
			}
			for (int i = id.Length; i < 6; i++)
				id = "0" + id;
		}

        id = cType + "_" + id;
        id += "_";
		foreach (int i in iTime) id += base62[i];
		id += "_";
		foreach (int i in iDate) id += base62[i];

		return id;
	}

	public void ReadInfoFromID(string id, string type)
	{
		string name = "";
		// string type = "";
		List<int> InstanceTime = new List<int>();
		string ID = id;

		//Define Type
		{
			string path = Path.Combine(Application.dataPath, "Data/TypesDictionary.json");
			string jsonText = File.ReadAllText(path);
			if(jsonText != null)
			{
                JArray typesDict = JArray.Parse(jsonText);
                foreach (JObject j in typesDict)
                    if (j["name"].ToString().Substring(0, 1) == ID.Substring(0, 1))
                        type = j["name"].ToString();
            }
		}
	
		ID = ID.Substring(2);
		ID = ID.Replace("_", "");

		for (string s = ID.Substring(0, 1); s == "0";)
		{
			ID = ID.Substring(1);
		}

        string[] sTimeContainter = { ID.Substring(6, 3), ID.Substring(9, 3) };
		if ((ID.Length - 6) == 6)
		{
            string[] sNameContainer = { ID.Substring(0, 3), ID.Substring(3, 3) };

            foreach (string s in sNameContainer)
            {
                foreach (char c in s)
                {
                    int ind = 0;
                    if (s == sNameContainer[0] && c == s[0])
                        ind = base62.IndexOf(c) + ('A' - 1);
                    else
                        ind = base62.IndexOf(c) + ('a' - 1);

                    name += (char)ind;
                }
            }
        }
		else
        {
			string sNameContainer = ID.Substring(0, ID.Length - 6);
            foreach (char c in sNameContainer)
            {
                int ind = 0;
                if (c == sNameContainer[0])
                    ind = base62.IndexOf(c) + ('A' - 1);
                else
                    ind = base62.IndexOf(c) + ('a' - 1);

                name += (char)ind;
            }
        }

        //Verify or Complet Name From Dictionary
        {
            dictManager.DictionaryVerification(); //From DictionaryManager

            string path = Path.Combine(Application.dataPath, "Data/" + type + "sDictionary.json");
            string jsonText = File.ReadAllText(path);
            if (jsonText != null)
            {
                JArray resourcesDict = JArray.Parse(jsonText);

                foreach (JObject j in resourcesDict)
                {
                    string Jname = j["name"].ToString();
					if (name.Length < 6)
					{
						if (Jname == name)
							name = Jname;
					}
					else
					{
                        if (Jname.Substring(0, 3) == name.Substring(0, 3) && Jname.Substring(Jname.Length - 3) == name.Substring(name.Length - 3))
                            name = Jname;
                    }
                }
            }
        }

        foreach (string s in sTimeContainter)
		{
			foreach (char c in s)
				InstanceTime.Add(base62.IndexOf(c));
		}
		InstanceTime[InstanceTime.Count - 1] = InstanceTime[InstanceTime.Count - 1] + 2000;
	}
}