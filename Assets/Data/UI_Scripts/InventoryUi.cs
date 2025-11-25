using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUi : MonoBehaviour
{
    // External References
    public UIDocument uiDocument;
    public Inventory _Inv;

    // UI Elements
    private VisualElement inventoryBar;
    private VisualElement[] slots;
    int selectedIndex = -1;

    private void OnEnable()
    {
        Def_Components();
        Def_BaseSet();
    }

    private void Start()
    {
        UpdateItemsBar();
    }

    private void Update()
    {
        UpdateItemsBar();
        DetectKeyboard();
    }
    void DetectKeyboard()
    {
        for (int i = 0;i <=9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                // Debug.Log("La tecla " + i + " fue precionada");
                int slotIndex = ( i == 0 ? 9 : i - 1 );
                SelectSlot(slotIndex);
            }
        }
    }
    void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        if (selectedIndex != -1)
        {
            slots[selectedIndex].RemoveFromClassList("selected");
        }

        selectedIndex = index;
        slots[selectedIndex].AddToClassList("selected");
    }
    void UpdateItemsBar()
    {
        for (int i = 0; i < _Inv.barItems.Length; i++)
        {
            if (_Inv.barItems[i] != null)
            {
                slots[i].style.backgroundImage = new StyleBackground(_Inv.barItems[i].icon);
            }
            else
                slots[i].style.backgroundImage = null;
        }
    }

    // OnEable Definitions
    public void Def_Components()
    {
        // Inv Def
        _Inv = GetComponent<Inventory>();

        // UI Def
        var root = uiDocument.rootVisualElement;

        inventoryBar = root.Q<VisualElement>("Inv_Bar_Cont");
        slots = new VisualElement[inventoryBar.childCount];

        for (int i = 0; i < slots.Length; i++)
            slots[i] = inventoryBar[i];
    }
    public void Def_BaseSet()
    {
        slots[0].AddToClassList("selected");
    }
}
