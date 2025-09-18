using UnityEngine;

public class MainChInventory : MonoBehaviour
{
    public struct Item
    {
        public int id;
        //public Image icon;
        public string name;
        public string description;
        public int quantity;

        public Item(int ID, /*Image Icon,*/ string Name, string Description, int Quantity)
        {
            id = ID;
            //icon = Icon;
            name = Name;
            description = Description;
            quantity = Quantity;
        }
    }
