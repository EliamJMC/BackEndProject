using UnityEngine;

public class ItemInitializator : MonoBehaviour
{
    private Item itemInfo =
        new Item(Type.Material)
        {
            name = "",
            description = "",
            properties = { },
            quality = 0,
            quantity = 0,
            itemSpecifications = new Material() 
            { 
                M_ID = "",
                transformMethod = ""
            }
        };
}
