using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public GameObject itemDropPrefab;
    public GameObject itemEquipPrefab;

    public virtual void PrimaryAction(InventoryManager inventoryManager){}
    public virtual void PrimaryActionHeld(InventoryManager inventoryManager){}
    public virtual void SecondaryAction(InventoryManager inventoryManager){}
}
