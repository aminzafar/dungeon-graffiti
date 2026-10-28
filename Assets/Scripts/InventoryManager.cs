using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<ItemData> inventoryItems = new List<ItemData>();
    [SerializeField] private int maxCapacity;

    [SerializeField] private Transform equipPoint;
    
    [SerializeField] public Camera playerCamera;
    [SerializeField] private float dropOffset = 1f;

    private ItemData currentEquippedItem;
    private GameObject currentEquippedModel;
    private int currentItemIndex = 0;

    private void Update()
    {
        if (currentEquippedItem == null) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropItem();
        }
        else if (Input.GetButtonDown("Fire1"))
        {
            currentEquippedItem.PrimaryAction(this);
        }
        else if (Input.GetButton("Fire1"))
        {
            currentEquippedItem.PrimaryActionHeld(this);
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            currentEquippedItem.SecondaryAction(this);
        }
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && inventoryItems.Count > 1)
        {
            if (scroll > 0f)
            {
                currentItemIndex--;
                if (currentItemIndex < 0) currentItemIndex = inventoryItems.Count - 1;
            }
            else if (scroll < 0f)
            {
                currentItemIndex++;
                if (currentItemIndex >= inventoryItems.Count) currentItemIndex = 0;
            }

            EquipItem(inventoryItems[currentItemIndex]);
        }
    }

    public void AddItem(ItemData itemToAdd)
    {
        inventoryItems.Add(itemToAdd);
        Debug.Log($"Added {itemToAdd.itemName} to Inventory. ({inventoryItems.Count}/{maxCapacity})");

        if (currentEquippedItem == null)
        {
            EquipItem(itemToAdd);
        }
    }

    private void EquipItem(ItemData itemToEquip)
    {
        currentEquippedItem = itemToEquip;

        if (currentEquippedModel != null)
        {
            Destroy(currentEquippedModel);
        }

        currentEquippedModel = Instantiate(itemToEquip.itemEquipPrefab, equipPoint);

        currentEquippedModel.transform.localPosition = Vector3.zero;
        currentEquippedModel.transform.localRotation = Quaternion.identity;
    }

    private void DropItem()
    {
        Vector3 spawnPosition = playerCamera.transform.position + (playerCamera.transform.forward * dropOffset);

        Instantiate(currentEquippedItem.itemDropPrefab, spawnPosition, playerCamera.transform.rotation);

        inventoryItems.RemoveAt(currentItemIndex);
        Debug.Log($"Dropped {currentEquippedItem.itemName} from Inventory. Items Left ({inventoryItems.Count}/{maxCapacity})");
        Destroy(currentEquippedModel);
        currentEquippedItem = null;

        if (inventoryItems.Count > 0)
        {
            if (currentItemIndex >= inventoryItems.Count) currentItemIndex = inventoryItems.Count - 1;
            currentEquippedItem = inventoryItems[currentItemIndex];
            currentEquippedModel = Instantiate(currentEquippedItem.itemEquipPrefab, equipPoint);
        }
    }

    public bool IsFull()
    {
        return inventoryItems.Count >= maxCapacity;
    }

    public void ThrowEquippedItem(float throwForce)
    {
        Vector3 spawnPosition = playerCamera.transform.position + (playerCamera.transform.forward * dropOffset);

        GameObject thrownItem = Instantiate(currentEquippedItem.itemDropPrefab, spawnPosition, playerCamera.transform.rotation);

        Rigidbody rb = thrownItem.GetComponent<Rigidbody>();
        rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

        inventoryItems.RemoveAt(currentItemIndex);
        Debug.Log($"Threw {currentEquippedItem.itemName}! ({inventoryItems.Count}/{maxCapacity})");
        Destroy(currentEquippedModel);
        currentEquippedItem = null;

        if (inventoryItems.Count > 0)
        {
            if (currentItemIndex >= inventoryItems.Count) currentItemIndex = inventoryItems.Count - 1;
            currentEquippedItem = inventoryItems[currentItemIndex];
            currentEquippedModel = Instantiate(currentEquippedItem.itemEquipPrefab, equipPoint);
        }
    }
}
