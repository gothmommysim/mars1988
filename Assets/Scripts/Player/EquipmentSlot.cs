using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItemManager;

public class EquipmentSlot : UISlot
{
    InventoryManager inventory;
    GameObject player;
    PlayerMovement playerMotion;
    [SerializeField] Storage equipStorage;

    public Pickup equippedPickup;
    public enum EquipTypes { primary, secondary };
    [SerializeField] public EquipTypes equipType;
    public bool isEquipSlot;
    public bool isEquipmentManaged;
    bool isItemSet;
    public bool equipStatusSet; //Reset every time action is performed

    Item savedItem;

    RawImage activeSlotImage;

    private void Awake()
    {
        if (equipType == EquipTypes.primary)
        {
            isEquipSlot = true;
        }
        else if (equipType == EquipTypes.secondary)
        {
            isEquipSlot = false;
        }
        player = transform.root.gameObject;

        slotContainer = Object.Instantiate(equipStorage);

        inventory = player.GetComponent<InventoryManager>();
        playerMotion = player.GetComponent<PlayerMovement>();
        activeSlotImage = transform.GetChild(0).GetComponent<RawImage>();
    }
    private void Update()
    {

        if (!storageBuilt)
        {
            //The slot is the storage
            Create.BuildStorage(slotContainer);
            storageBuilt = true;
        }
        SettingItem();

        EquipmentManagement();
        ShowActiveSlot();
    }
    private void SettingItem()
    {
        if (slotContainer.storageSlots[localX, localY].item != null && !isItemSet)
        {
            item = slotContainer.storageSlots[localX, localY].item;
            savedItem = item;
            if (item.inventoryItem == null)
            {
                inventory.DrawInventoryItem(item, transform);
                item.inventoryItem.EquipmentStorageValues();
            }

            isItemSet = true;
        }
        else if (slotContainer.storageSlots[localX, localY].item == null && isItemSet)
        {
            item = null;
            isItemSet = false;
        }
    }
    private void ShowActiveSlot()
    {
        if (isEquipSlot)
        {
            activeSlotImage.enabled = true;
        }
        else
        {
            activeSlotImage.enabled = false;
        }
    }
    public void ClearSlot()
    {
        equippedPickup = null;
        isEquipmentManaged = false;
        equipStatusSet = false;
    }
    private void EquipmentManagement()
    {
        if (isItemSet)   //If there is an item in the slot / Added an item
        {
            if (!isEquipmentManaged && item.previousStorage.storageType != Storage.StorageTypes.equipStorage) //inventory to equipment slot
            {
                Debug.Log("Moved to equip slot from inventory");
                EquipSlotFromInventory();
            }

            else if (!isEquipmentManaged && item.previousStorage.storageType == Storage.StorageTypes.equipStorage)
            {
                MoveFromEquipSlot();
            }
        }
        else if (!isItemSet && isEquipmentManaged) //equipment has been managed but we not longer have an item, so we should clear
        {
            Debug.Log("Clearing slot");
            ClearSlot();
        }

        CheckEquip();
    }

    private void CheckEquip()
    {
        if (equippedPickup != null && !equipStatusSet)        //If item is equipped in the slot but slot has not been assigned check yet
        {
            Debug.Log("Checking equip");

            if (isEquipSlot)                                //If is current equip slot
            {
                inventory.WeaponSwitchToHands(equippedPickup);
                equipStatusSet = true;
            }
            else
            {

                inventory.WeaponSwitchToBack(equippedPickup);
                equipStatusSet = true;

            }
        }
        else if (isEquipSlot && !equipStatusSet && !isItemSet)
        {
            if (savedItem != null)
            {
                if (savedItem.currentStorage != null)
                {
                    playerMotion.onDiscard();
                    equipStatusSet = true;
                }
            }
        }
    }
    public void MoveFromEquipSlot()
    {
        item.instancedObj.TryGetComponent<Pickup>(out equippedPickup);

        Debug.Log("moved from equip slot");
        equipStatusSet = false;

        isEquipmentManaged = true;
    }

    public void GroundToEquip(Pickup pickup)
    {
        equipStatusSet = false;
        var uniqueItem = pickup.uniqueItem;

        inventory.PlaceItem(0, 0, slotContainer, uniqueItem);
        isEquipmentManaged = true;

        var tempItem = inventory.DrawInventoryItem(uniqueItem, transform);
        tempItem.EquipmentStorageValues();

        equippedPickup = pickup;
        Debug.Log(equippedPickup);
    }

    public void EquipSlotFromInventory() //spawn item after timer
    {
        equipStatusSet = false;

        if (item.instancedObj != null)
        {
            Debug.Log("Instance already set, setting active");
            item.instancedObj.TryGetComponent<Pickup>(out equippedPickup);
            item.instancedObj.SetActive(true);
        }
        else
        {
            Debug.Log("Creating new physical item");

            equippedPickup = Create.BuildPhysicalItem(player.transform, item);
            equippedPickup.EnablePlayerTransforms(player);
        }

        isEquipmentManaged = true;
    }
}
