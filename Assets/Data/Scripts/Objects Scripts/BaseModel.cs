using System;
using UnityEngine;

public enum Type 
{ 
    RawResource, 
    Material, 
    Part, 
    Drops,
    Weapon, 
    Armor, 
    Consumable
}
public enum WeaponType 
{
    Improvised,
    Crafted,
    Idustrial
}
public enum ArmorType 
{
    Cloths,
    Crafted,
    Idustrial,
    Modified
}
public enum ConsomableType 
{
    Raw,
    Cooked,
    Drink,
    Snack,
    Canned,
    Spoiled
}

// Item type
[System.Serializable]
public struct RawResource {
    public string R_ID;
    public string source;
    public string extracMethod;
}
[System.Serializable]
public struct Material {
    public string M_ID;
    public string transformMethod;
}
[System.Serializable]
public struct Part {
    public string P_ID;
    public string basePiece;
    public string extracMethod;
}
[System.Serializable]
public struct Drop {
    public string D_ID;
    public string baseEntitie;
    public string colectionMethod;
}
[System.Serializable]
public struct Weapon {
    public string W_ID;
    public string[] materials;
    public int weaponDamage;
    public int durability;
    public WeaponType weaponType;
}
[System.Serializable]
public struct Armor {
    public string A_ID;
    public string[] materials;
    public int protection;
    public int durability;
    public ArmorType armorType;
}
[System.Serializable]
public struct Consumable {
    public string C_ID;
    public int calories;
    public int protein;
    public int carbs;
    public int fat;
    public int hydratation;
    public float expirationTime;
    public ConsomableType consomableType;
}

[System.Serializable]
public class Item
{
    public string name;
    public string description;
    public string[] properties;
    public int quality;
    public int quantity;
    public Type type;

    public int maxStack;
    public float weight;

    public RawResource? rawResource;
    public Material? material;
    public Part? part;
    public Drop? drop;
    public Weapon? weapon;
    public Armor? armor;
    public Consumable? consumable;

    public Item(Type type) {
        this.type = type;
        
        switch (type) {
            // Items by 1 Type
            case Type.RawResource:
                weight = 0.2f;
                maxStack = 10 / weight;
                rawResource = new RawResource();
                break;
                
            case Type.Material:
                weight = 0.1f;
                maxStack = 10 / weight;
                material = new Material();
                break;

            case Type.Part:
                weight = 0.1f;
                maxStack = 10 / weight;
                part = new Part();
                break;

            case Type.Drops: 
                weight = 0.2f;
                maxStack = 10 / weight;
                drop = new Drop();
                break;

            // Items by 2 Types
            case Type.Weapon:
                weapon = new Weapon();
                switch (weapon.weaponType)
                {
                    case WeaponType.Improvised: weight = 0.5f; maxStack = 4; break;
                    case WeaponType.Crafted:    weight = 0.8f; maxStack = 2; break;
                    case WeaponType.Idustrial:  weight = 1.0f; maxStack = 1; break;
                }
                break;

            case Type.Armor:
                armor = new Armor();
                switch (armor.armorType)
                {
                    case ArmorType.Cloths:      weight = 0.3f; break;
                    case ArmorType.Crafted:     weight = 0.5f; break;
                    case ArmorType.Idustrial:   weight = 1.0f; break;
                    case ArmorType.Modified:    weight = 0.8f; break;
                }
                break;

            case Type.Consumable:
                consumable = new Consumable();
                switch (consumable.consomableType) 
                {
                    case ConsomableType.Raw:     weight = 0.1f; break;
                    case ConsomableType.Cooked:  weight = 0.2f; break;
                    case ConsomableType.Drink:   weight = 0.3f; break;
                    case ConsomableType.Snack:   weight = 0.1f; break;
                    case ConsomableType.Canned:  weight = 0.3f; break;
                    case ConsomableType.Spoiled: weight = 0.2f; break;
                }
                break;
        }
    }

/* 
    Item by 1 type creation

        Item wood = new Item(Type.RawResource)
        {
            name = "Wood";
            description = "RawWood";
            properties = { 
                "inflamable", 
                "hard", 
                "flexible" 
            };
            quality = 3;
            quantity = 5;
            rawResource = new RawResource
            {
                R_ID = "Wood_01";
                source = "Oak Forest";
                extracMethod = "Cutted with axe";
            }
        };

    Item by 2 types creation

        Item apple = new Item(Type.Consumable);
        {
            name = "Apple";
            description = "Fresh red apple, restores a little health.";
            properties = {
                "Heal",
                "tempIncreasStamina",
                "tempIncreasSpeed",
                "tempIncreasStats"
            }
            quantity = 5;
            quality = 2;
            consumable = new Consumable
            {
                C_ID = "apple_01",
                calories = 52,
                protein = 0,
                carbs = 14,
                fat = 0,
                hydratation = 20,
                expirationTime = 72f,
                consomableType = ConsomableType.Raw
            }
        }

*/
}
