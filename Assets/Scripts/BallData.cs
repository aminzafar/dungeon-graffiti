using UnityEngine;

[CreateAssetMenu(fileName = "New Ball", menuName = "Inventory/Ball")]
public class BallData : ItemData
{
    public float throwForce;

    public override void PrimaryAction(InventoryManager inventoryManager)
    {
        inventoryManager.ThrowEquippedItem(throwForce);
    }
}
