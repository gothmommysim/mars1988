using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ItemManager;
using TMPro;

public class InventoryItem : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{

    public Item item;
    InventoryManager inventory;
    Canvas canvas;
    RectTransform itemRectTransform;
    CanvasGroup canvasGroup;
    Vector2 leftAnchor;
    Vector2 centeredValues;
    Vector2 initialPivot;
    Vector2 savedPivot;
    Vector2 initialItemPos;
    float initialItemRotation; //Stored as the item rotating along the z axis

    UISlot prevSlot;
    UISlot currentSlot;

    bool isDisabled = false;
    bool movingItem = false;
    bool sizeSet = false;

    bool itemRotated = false; //So holding rotate doesn't spin the item constantly

    void Awake()
    {
        itemRectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
        inventory = transform.root.GetComponent<InventoryManager>();
        centeredValues = new Vector2(0.5f, 0.5f);
        leftAnchor = itemRectTransform.anchorMin;

        if (item is Storage)
        {
            //gameObject.AddComponent<MobileStorage>();
        }
    }

    void ShowItemDisabled()
    {
        //fix -- causes disabled weapon switch bug
        if (!isDisabled && (inventory.playerMotion.switchQueue && item.currentStorage.storageType==Storage.StorageTypes.equipStorage))
        {
            canvasGroup.alpha = .8f;
            isDisabled = true;
        }
        else if (isDisabled && !inventory.playerMotion.switchQueue)
        {
            canvasGroup.alpha = 1f;
            isDisabled = false;
        }
    }
    void Update()
    {
        SetDefaultSize();

        ShowItemDisabled();

        if (movingItem)
        {
            Rotate();
        }
        if (item.inventoryItem == null)
        {
            TryGetComponent<InventoryItem>(out var tempItem);
            item.inventoryItem = tempItem;
        }
    }
    void RotateItemVals()
    {
        //Rotate values in item
        int temp = 0;
        temp = item.itemWidth;
        item.itemWidth = item.itemHeight;
        item.itemHeight = temp;
    }
    void ResetRotation()
    {
        if (Mathf.Round(Mathf.Abs(Mathf.Abs(item.itemRotation) - Mathf.Abs(initialItemRotation))) == 90)  //Reset size values in item if the change is a 90 degree change
        {
            RotateItemVals();
        }

        //Reset pivot
        itemRectTransform.pivot = initialPivot;

        //Reset rotation
        item.itemRotation = initialItemRotation;

        itemRectTransform.SetPositionAndRotation(
                        itemRectTransform.position,
                        Quaternion.Euler(itemRectTransform.rotation.eulerAngles.x, itemRectTransform.rotation.eulerAngles.y, item.itemRotation));
    }
    void UIRotation()
    {
        //Rotate ui element
        itemRectTransform.SetPositionAndRotation(
                        itemRectTransform.position,
                        Quaternion.Euler(itemRectTransform.rotation.eulerAngles.x, itemRectTransform.rotation.eulerAngles.y, item.itemRotation));
        if (item.itemRotation == 0)
        {
            savedPivot = new Vector2(0f, 1f);
        }
        else if (item.itemRotation == 90)
        {
            savedPivot = new Vector2(1f, 1f);
        }
        else if (item.itemRotation == -90)
        {
            savedPivot = new Vector2(0f, 0f);
        }
        else if (Mathf.Abs(item.itemRotation) == 180)
        {
            savedPivot = new Vector2(1f, 0f);
        }
    }
    public void SetItemPosition()
    {
        //Set new pivot (a check for if the item has rotated), otherwise sets it to initial
        itemRectTransform.pivot = savedPivot;

        //Check if storage is the same instance
        if (item.currentStorage.storageType == Storage.StorageTypes.equipStorage || item.currentStorage.storageType == Storage.StorageTypes.storeStorage)
        {
            transform.SetParent(currentSlot.transform);
            EquipmentStorageValues(); Debug.Log(currentSlot.transform);
        }
        else if (!item.currentStorage.Equals(item.previousStorage)) //moving to new storage area
        {
            if (item.previousStorage.storageType == Storage.StorageTypes.equipStorage || item.previousStorage.storageType == Storage.StorageTypes.storeStorage)
            {
                ItemStorageValues();
            }
            transform.SetParent(currentSlot.transform.parent.parent.Find("storageAreaItems")); //Set parent to storage area items location
        }

        itemRectTransform.anchoredPosition = new Vector2(initialItemPos.x + InventoryManager.CellSize * (item.slotX - item.prevSlotX), initialItemPos.y - InventoryManager.CellSize * (item.slotY - item.prevSlotY));
    }
    void ResetItem()
    {

        if (!initialPivot.Equals(savedPivot)) // If the initial pivot changed, reset the rotation
        {
            ResetRotation();
        }
        else
        {
            itemRectTransform.pivot = initialPivot; // if not just set the pivot to the original pivot
        }
        /*if (item.currentStorage != null)
        {
            if (prevSlot != null)
            {
                inventory.PlaceItem(prevSlot.localX, prevSlot.localY, item.currentStorage, item);
            }
            else
            {
                inventory.PlaceItem(item.prevSlotX, item.prevSlotY, item.currentStorage, item);
            }
        }*/
        inventory.PlaceItem(item.prevSlotX, item.prevSlotY, item.previousStorage, item);

        itemRectTransform.anchoredPosition = initialItemPos;
    }
    void Rotate()
    {
        Keyboard k = InputSystem.GetDevice<Keyboard>();
        if (k.rKey.isPressed && !itemRotated)
        {
            //Rotate values in item
            RotateItemVals();

            //Set new item rotation
            item.itemRotation = itemRectTransform.rotation.eulerAngles.z - 90f;

            UIRotation();
            itemRotated = true;
        }
        else if (!k.rKey.isPressed)
        {
            itemRotated = false;
        }
    }
    public void EquipmentStorageValues()
    {
        if (!sizeSet)
        {
            SetDefaultSize();
        }
        //Make item upright
        item.itemRotation = 0;
        UIRotation();
        if (item.itemHeight > item.itemWidth)
        {
            var temp = item.itemHeight;
            item.itemHeight = item.itemWidth;
            item.itemWidth = temp;
        }

        itemRectTransform.anchorMax = centeredValues;
        itemRectTransform.anchorMin = centeredValues;
        itemRectTransform.pivot = centeredValues;
    }
    void ItemStorageValues()
    {
        itemRectTransform.anchorMax = leftAnchor;
        itemRectTransform.anchorMin = leftAnchor;
        UIRotation();
        itemRectTransform.pivot = savedPivot;
    }
    void SetDefaultSize()
    {
        if (item != null && !sizeSet)
        {
            itemRectTransform.anchoredPosition = new Vector2(itemRectTransform.anchoredPosition.x + InventoryManager.CellSize * item.slotX, itemRectTransform.anchoredPosition.y - InventoryManager.CellSize * item.slotY);

            if (Mathf.Abs(item.itemRotation) == 0 || Mathf.Abs(item.itemRotation) == 180)
            { //if item hasn't been rotated
                //Change to size of item
                itemRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, item.itemWidth * InventoryManager.CellSize);
                itemRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, item.itemHeight * InventoryManager.CellSize);
            }
            else
            {
                itemRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, item.itemHeight * InventoryManager.CellSize);
                itemRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, item.itemWidth * InventoryManager.CellSize);
            }
            UIRotation();
            itemRectTransform.pivot = savedPivot;

            sizeSet = true;
        }
        else if (item == null && sizeSet)
        {
            sizeSet = false;
        }
    }
    public void BeginMove()
    {
        //Set initial slot and ui position

        initialItemPos = itemRectTransform.anchoredPosition;
        initialItemRotation = item.itemRotation;
        initialPivot = itemRectTransform.pivot;

        //Create save of pivot as initial in case we don't rotate
        savedPivot = initialPivot;
        itemRectTransform.pivot = centeredValues;

        inventory.RemoveItem(item); //REPLACE ITEM ON DISCONNECT OR APPLICATION CLOSE
    }

    public void OnBeginDrag(PointerEventData pointer)
    {
        if (pointer.button == PointerEventData.InputButton.Left)
        {
            BeginMove();

            canvasGroup.alpha = .8f;
            canvasGroup.blocksRaycasts = false;
            canvas.overrideSorting = true;

            movingItem = true;
        }
    }
    public void OnDrag(PointerEventData pointer)
    {
        if (pointer.button == PointerEventData.InputButton.Left && !isDisabled)
        {
            itemRectTransform.position =
            new Vector3(pointer.position.x,
                        pointer.position.y,
                        itemRectTransform.position.z);
        }

    }
    public void OnEndDrag(PointerEventData pointer)
    {
        if (pointer.button == PointerEventData.InputButton.Left)
        {

            if (pointer.pointerCurrentRaycast.isValid && !isDisabled)
            {
                //Slot we are dragging to
                pointer.pointerCurrentRaycast.gameObject.TryGetComponent<UISlot>(out currentSlot);

                if (currentSlot != null)
                {
                    //Check if ending location or rotation is the same as our initial, if it is then we don't need to check the slot
                    if (currentSlot.localX != item.prevSlotX || currentSlot.localY != item.prevSlotY || currentSlot.slotContainer != item.previousStorage || !initialPivot.Equals(savedPivot))
                    {
                        // if we can fit the item then place it, otherwise send it back to the original slot
                        if (inventory.CheckSlotFit(currentSlot.localX, currentSlot.localY, currentSlot.slotContainer, item, currentSlot))
                        {
                            Debug.Log("Placing item");
                            inventory.PlaceItem(currentSlot.localX, currentSlot.localY, currentSlot.slotContainer, item);
                            SetItemPosition();
                        }
                        else
                        {
                            Debug.Log("Item doesn't fit");
                            //Reset rotation and position if rotated into a slot that doesn't fit
                            ResetItem();
                        }
                    }
                    else
                    {
                        Debug.Log("End slot same as original slot");
                        ResetItem();
                    }
                }
                else
                {
                    Debug.Log("End slot null inside of inventory");
                    //Reset rotation if rotated out of bounds
                    ResetItem();
                }
            }
            else
            {
                Debug.Log("End slot null outside of inventory");
                //Reset rotation if rotated out of bounds
                ResetItem();
            }

            canvasGroup.alpha = 1f;
            //Turn back on raycast with item at end so that we can read slots
            canvasGroup.blocksRaycasts = true;
            canvas.overrideSorting = false;
            movingItem = false;
        }
    }
    public void OnPointerDown(PointerEventData pointer)
    {
        //Cursor.visible = false;
    }

    public void OnPointerUp(PointerEventData pointer)
    {
        if (pointer.button == PointerEventData.InputButton.Right && !(item.currentStorage.storageType == Storage.StorageTypes.equipStorage))
        {
            
            
            inventory.DisableActionMenu(); //ADD THIS TO INVENTORY CODE (MENU NEEDS TO REFRESH ON NEW CLICK)

            inventory.selectedInventoryItem = this; //select itself

            var menuRectTransform = inventory.actionMenu.GetComponent<RectTransform>();

            menuRectTransform.position =
            new Vector3(pointer.position.x,
            pointer.position.y,
            itemRectTransform.position.z);

            
            if (transform.root.tag.Equals("Player")&& item.baseObj!=null)
            {
                inventory.dropButton.SetActive(true);
            }
            if ((item is Weapon || item.canEquip) && (inventory.primarySlot.item == null || inventory.secondarySlot.item == null))
            {
                //Activate equip button
                inventory.equipButton.SetActive(true);
            }
            if (item is Item)
            {
                inventory.infoButton.SetActive(true);
            }
        }
        else
        {
            inventory.DisableActionMenu();
        }
        //Cursor.visible = true;
    }

    public void OnDrop(PointerEventData pointer)
    {

    }

    public void EquipMenuButton(InventoryManager inventory)
    {
        //If primary slot is empty and secondary isn't the equip or full and we arent switching weps, move it in
        if ((inventory.primarySlot.isEquipSlot || (inventory.secondarySlot.isEquipSlot && inventory.secondarySlot.equippedPickup != null)) && inventory.primarySlot.equippedPickup == null && !inventory.playerMotion.switchQueue)
        {
            Debug.Log("To primary");
            //move from inventory to primary
            inventory.selectedInventoryItem.BeginMove();

            inventory.PlaceItem(inventory.primarySlot.localX, inventory.primarySlot.localY, inventory.primarySlot.slotContainer, inventory.selectedInventoryItem.item);

            inventory.selectedInventoryItem.currentSlot = inventory.primarySlot;
            inventory.selectedInventoryItem.SetItemPosition();
            
            inventory.primarySlot.item = inventory.selectedInventoryItem.item;
            inventory.primarySlot.EquipSlotFromInventory();
        }
        //If secondary slot is empty and player isn't switching weps
        else if (inventory.secondarySlot.equippedPickup == null && !inventory.playerMotion.switchQueue)
        {
            Debug.Log("To secondary");
            //move from inventory to secondary
            inventory.selectedInventoryItem.BeginMove();

            inventory.PlaceItem(inventory.secondarySlot.localX, inventory.secondarySlot.localY, inventory.secondarySlot.slotContainer, inventory.selectedInventoryItem.item);
            inventory.selectedInventoryItem.currentSlot = inventory.secondarySlot;
            inventory.selectedInventoryItem.SetItemPosition();

            inventory.secondarySlot.item = inventory.selectedInventoryItem.item;
            inventory.secondarySlot.EquipSlotFromInventory();
        }
        inventory.DisableActionMenu();
    }
    public void DropMenuButton(InventoryManager inventory)
    {
        var instancedPickup = Create.BuildPhysicalItem(inventory.dropLocation, inventory.selectedInventoryItem.item);
        instancedPickup.EnablePlayerTransforms(inventory.player);
        instancedPickup.DropItem(inventory.selectedInventoryItem);

        inventory.DisableActionMenu();
    }

    public void InfoMenuButton(InventoryManager inventory)
    {
        inventory.itemInfoBox.SetActive(true);
        TextMeshProUGUI itemNameText = inventory.itemInfoBox.transform.Find("itemName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemDescText =  inventory.itemInfoBox.transform.Find("itemDesc").GetComponent<TextMeshProUGUI>();

        itemNameText.text = inventory.selectedInventoryItem.item.itemName;
        itemDescText.text = inventory.selectedInventoryItem.item.itemDesc;

        inventory.DisableActionMenu();
    }

}
