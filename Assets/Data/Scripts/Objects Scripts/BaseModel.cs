using System;
using UnityEngine;

[System.Serializable]
public class BaseModel : MonoBehaviour
{
    public string id;
    public string name;
    public string description;
    public string type;
    public int quantity;
}

public class Item : BaseModel
{
    public string itemType;
}

public class Resource : BaseModel
{
    public string resourceType;
    public string[] sources;
}

public class Weapon : BaseModel
{
    public string weaponType;
}

public class Artefact : BaseModel
{
    public string artefactType;
}