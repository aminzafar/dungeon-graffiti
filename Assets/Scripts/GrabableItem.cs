using UnityEngine;

public class GrabableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    public void Interact(GameObject interactor)
    {
        InventoryManager playerInventory = interactor.GetComponent<InventoryManager>();

        if (playerInventory.IsFull())
        {
            Debug.Log("No more Space in the Inventory!");
            return;
        }
        
        playerInventory.AddItem(itemData);

        Destroy(gameObject);
    }
}