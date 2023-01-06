using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItemManager;

public class StorageSlot : UISlot
{
    InventoryManager inventory;
    [SerializeField] Storage storeStorage;
    public bool showingStorage = false;
    bool storageDrawn = false;
    bool isItemSet;

    [SerializeField] Item testItem;
    [SerializeField] Item testInsert;

    private void Awake()
    {

        slotContainer = Object.Instantiate(storeStorage);

        inventory = transform.root.GetComponent<InventoryManager>();

    }

    void Update()
    {
        if (!storageBuilt)
        {
            //The slot is the storage
            Create.BuildStorage(slotContainer);
            storageBuilt = true;

            //Testing
            inventory.PlaceItem(0, 0, slotContainer, Create.BuildItem(testItem));

        }

        if (slotContainer.storageSlots[localX, localY].item != null && !isItemSet) //item added to the slot
        {
            item = slotContainer.storageSlots[localX, localY].item;

            if (item.inventoryItem == null)
            {
                inventory.DrawInventoryItem(item, transform);
                item.inventoryItem.EquipmentStorageValues();
            }
            //TESTING
            if (testInsert != null && inventory.CheckSlotFit(0, 0, item as Storage, testInsert))
            {
                inventory.PlaceItem(0, 0, item as Storage, Create.BuildItem(testInsert));
            }
            Debug.Log((item as Storage).storageSlots.Length);
            //Testing

            isItemSet = true;
        }
        else if (slotContainer.storageSlots[localX, localY].item == null && isItemSet) //item removed from storage slot
        {
            item = null;
            isItemSet = false;
        }

        DrawStorage();
    }
    public void ShowStorageButton(Text text)
    {
        if (showingStorage == false)
        {
            text.text = "<";
            showingStorage = true;
        }
        else
        {
            text.text = ">";
            showingStorage = false;
        }
    }
    private void DrawStorage()
    {
        //If there is an item in the storage slot
        if (item != null)
        {
            //Draw storage if we want to show it, it hasn't been drawn, and the inventory has been fully built
            if (showingStorage && !storageDrawn)
            {
                //Draw storage ui (pass in horizontal panel group of the slot area)
                storageDrawn = inventory.DrawStorageUI(transform.parent.gameObject, item as Storage);
            }
            else if (!showingStorage && storageDrawn) //Clear storage if we don't want to show it and it's been drawn
            {
                storageDrawn = inventory.ClearStorageUI(transform.parent.gameObject);
            }
        }
        //If there is no longer an item in the storage slot
        else if (item == null)
        {
            //Clear the UI
            if (storageDrawn)
            {
                storageDrawn = inventory.ClearStorageUI(transform.parent.gameObject);
            }
        }
    }
}
