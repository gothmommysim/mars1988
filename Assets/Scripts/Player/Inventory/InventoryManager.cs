using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ItemManager;

public class InventoryManager : MonoBehaviour
{
    /*Under horizontal panel:
    0. storageSlot
    1. storageButton
    2. storageAreaSlots
    3. storageAreaItems
     */


    public GameObject player;
    public PlayerMovement playerMotion;
    public GameObject inventoryUI;
    public GameObject inventoryItem;
    public EquipmentSlot primarySlot;
    public EquipmentSlot secondarySlot;
    bool inventoryEnabled;
    public float UIDuration = .07f;
    [SerializeField] private UISlot uiSlot;
    [SerializeField] private InventoryItem uiItem;

    [Header("Action Menu")]
    public GameObject actionMenu;
    public GameObject equipButton;
    public GameObject infoButton;
    public GameObject useButton;
    public GameObject dropButton;
    public Transform dropLocation; // drop transform

    [Header("Item Info")]
    public GameObject itemInfoBox;

    public InventoryItem selectedInventoryItem;

    public Transform equipItemTransform;
    public Transform onBackTransform;

    //Default slot size
    public const int CellSize = 80;
    public const int equipCellSize = 96;

    void Awake()
    {
        DisableActionMenu();

        player = transform.root.gameObject;
        playerMotion = player.GetComponent<PlayerMovement>();

        //Primary slot transform
        primarySlot = inventoryUI.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<EquipmentSlot>();
        //Secondary slot transform
        secondarySlot = inventoryUI.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<EquipmentSlot>();

        //Disable inventory on wake
        inventoryUI.GetComponent<CanvasGroup>().alpha = 0f;
        inventoryEnabled = false;
    }
    public bool CheckSlotFit(int slotX, int slotY, Storage storage, Item item, UISlot endSlot = null)
    {   //Returns true if item fits in slot
        if (!storage.storageSlots[slotX, slotY].slotFilled)
        {
            if (storage.storageType == Storage.StorageTypes.equipStorage) //If equipment slot
            {
                if (item is Weapon || item.canEquip)
                {

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (storage.storageType == Storage.StorageTypes.storeStorage) //If storage slot
            {
                if (item is Storage)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                for (int y = 0; y < item.itemHeight; y++)
                {
                    for (int x = 0; x < item.itemWidth; x++)
                    {
                        if ((slotX + x < storage.storageWidth) && (slotY + y < storage.storageHeight)) //Check bounds
                        {
                            if (storage.storageSlots[slotX + x, slotY + y].slotFilled)
                            {
                                //Breakout if slot is within bounds but the slot is filled
                                return false;
                            }
                        }
                        else
                        {
                            //Breakout if a slot is out of bounds
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        return false;
    }

    public void CheckInventoryFit(Pickup pickup)
    {
        if (pickup.uniqueItem is Weapon || pickup.uniqueItem.canEquip)
        {
            //If primary slot is empty and is secondary isn't the equip or full and we arent switching weps, move it in
            if ((primarySlot.isEquipSlot || (secondarySlot.isEquipSlot && secondarySlot.item != null)) && primarySlot.item == null && !playerMotion.switchQueue)
            {
                pickup.DisableItemPhysics();
                primarySlot.GroundToEquip(pickup);
                pickup.itemPicked = true;
            }
            //If secondary slot is empty and player isn't switching weps
            else if (secondarySlot.item == null && !playerMotion.switchQueue)
            {
                pickup.DisableItemPhysics();
                secondarySlot.GroundToEquip(pickup);
                pickup.itemPicked = true;
            }
            //else, will be moved to backpack if there is space
        }
        else if (pickup.uniqueItem is Storage)
        {
            pickup.DisableItemPhysics();
            pickup.itemPicked = true;
        }
    }
    void CheckForPhysicalInstance(Item item)
    {
        if (item.instancedObj != null)
        {
            Debug.Log("Moved to inventory from spawned instance");
            //If item has been spawned move it to inventory
            if (item.instancedObj.TryGetComponent<Pickup>(out var pickup))
            {
                pickup.MoveToInventory();
            }
        }
        else
        {
            Debug.Log("No physical item exists");
        }
    }
    public void PlaceItem(int slotX, int slotY, Storage storage, Item item)
    {   /* Take in x and y for coordinates in inventory to place item
        * Take in current storage being accessed
        * Take in item to place at coordinates*/

        //Set the item

        //Add item to inventory
        storage.storageSlots[slotX, slotY].item = item;

        //Store inventory information on item
        item.currentStorage = storage;
        item.slotX = slotX;
        item.slotY = slotY;

        if (item.currentStorage.storageType == Storage.StorageTypes.itemStorage)
        {
            if (item.previousStorage != null)
            {
                if (!(item.previousStorage.storageType == Storage.StorageTypes.itemStorage)) //If item is not coming from another item storage
                {
                    CheckForPhysicalInstance(item);
                }
            }
            //Fill the slots
            for (int y = 0; y < item.itemHeight; y++)
            {
                for (int x = 0; x < item.itemWidth; x++)
                {
                    storage.storageSlots[slotX + x, slotY + y].slotFilled = true;
                }
            }

        }
        else
        {
            storage.storageSlots[slotX, slotY].slotFilled = true;
        }
    }
    public void RemoveItem(Item item)//, UISlot initialSlot = null)
    {
        //Removes item and returns item removed (item has to go somewhere)

        //Save previous storage information
        item.previousStorage = item.currentStorage;
        item.prevSlotX = item.slotX;
        item.prevSlotY = item.slotY;

        Debug.Log(item.currentStorage);
        //Unfill the slots
        if (item.currentStorage.storageType is Storage.StorageTypes.equipStorage
        || item.currentStorage.storageType is Storage.StorageTypes.storeStorage)
        {
            item.currentStorage.storageSlots[item.slotX, item.slotY].slotFilled = false;
        }
        else
        {
            for (int y = 0; y < item.itemHeight; y++)
            {
                for (int x = 0; x < item.itemWidth; x++)
                {
                    Debug.Log("Slot " + (item.slotX + x) + " " + (item.slotY + y) + " removed.");
                    item.currentStorage.storageSlots[item.slotX + x, item.slotY + y].slotFilled = false;
                }
            }
        }

        //Remove the item from the slot
        item.currentStorage.storageSlots[item.slotX, item.slotY].item = null;

        item.currentStorage = null;
    }


    private IEnumerator ShowInventory()
    {
        //inventoryUI.SetActive(true);
        float timeElapsed = 0f;
        float startAlpha = inventoryUI.GetComponent<CanvasGroup>().alpha;

        while (timeElapsed < UIDuration)
        {
            inventoryUI.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(startAlpha, 1f,
                timeElapsed / UIDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        inventoryUI.GetComponent<CanvasGroup>().alpha = 1f;
    }
    IEnumerator HideInventory()
    {
        float timeElapsed = 0f;
        float startAlpha = inventoryUI.GetComponent<CanvasGroup>().alpha;

        while (timeElapsed < UIDuration)
        {
            inventoryUI.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(startAlpha, 0f,
                timeElapsed / UIDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        inventoryUI.GetComponent<CanvasGroup>().alpha = 0f;
        //inventoryUI.SetActive(false);
    }
    void EnableInventory()
    {
        StartCoroutine(ShowInventory());

        DisableActionMenu();
        inventoryUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
        playerMotion.hasCameraControl = false;
        playerMotion.allowFire = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    void DisableInventory()
    {
        StartCoroutine(HideInventory());

        DisableActionMenu();
        inventoryUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
        playerMotion.hasCameraControl = true;
        playerMotion.allowFire = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void DisableActionMenu()
    {
        infoButton.SetActive(false);
        useButton.SetActive(false);
        equipButton.SetActive(false);
        dropButton.SetActive(false);
    }
    public bool DrawStorageUI(GameObject container, Storage currentStorage)
    {   //Returns bool for the storage being drawn
        //Change width and height of storageArea

        var storageAreaLayoutElement = container.transform.GetChild(2).GetComponent<LayoutElement>();

        storageAreaLayoutElement.preferredWidth = CellSize * currentStorage.storageWidth;
        storageAreaLayoutElement.preferredHeight = CellSize * currentStorage.storageHeight;


        //Draw slots
        for (int y = 0; y < currentStorage.storageHeight; y++)
        {
            for (int x = 0; x < currentStorage.storageWidth; x++)
            {
                //Instantiate temporary UI elements to represent inventory
                UISlot tempSlot = Instantiate(uiSlot, container.transform.GetChild(2)); //Instantiate UI slot within storage area
                tempSlot.localX = x;
                tempSlot.localY = y;

                tempSlot.slotContainer = currentStorage;
                if (currentStorage.storageSlots[x, y].item != null)
                {
                    tempSlot.item = currentStorage.storageSlots[x, y].item;
                    DrawInventoryItem(currentStorage.storageSlots[x, y].item, container.transform.GetChild(3));
                }
            }
        }
        //Storage is being drawn
        return true;
    }
    public bool ClearStorageUI(GameObject container)
    {   //Returns bool for the storage being cleared
        //Remove UI elements
        DisableActionMenu();

        var storageAreaLayoutElement = container.transform.GetChild(2).GetComponent<LayoutElement>();

        storageAreaLayoutElement.preferredWidth = 0;
        storageAreaLayoutElement.preferredHeight = 0;

        foreach (Transform child in container.transform.GetChild(2))
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in container.transform.GetChild(3))
        {
            GameObject.Destroy(child.gameObject);
        }
        //Storage is no longer drawn
        return false;
    }

    public InventoryItem DrawInventoryItem(Item item, Transform uiParent)
    {
        if (item.inventoryItem == null)
        {
            
            var tempItem = Instantiate(uiItem, uiParent);
            tempItem.item = item;
            tempItem.GetComponent<RawImage>().texture = tempItem.item.itemThumbnail;
            if (tempItem.item.currentStorage.storageType == Storage.StorageTypes.storeStorage) {
                Debug.Log("Drawn inventory item");
                tempItem.GetComponent<RectTransform>().localScale = new Vector3(0f,0f,0f);
            }
            item.inventoryItem = tempItem;
            return tempItem;
        }
        else
        {
            return item.inventoryItem;
        }
    }

    void SwitchWeapons()
    {

    }
    //Equip item into hands
    IEnumerator EquipItem(Pickup pickup)
    {
        if (pickup.itemCollision.enabled)
        {
            pickup.DisableItemPhysics();
        }
        playerMotion.onPickup();
        pickup.transform.parent = equipItemTransform;

        yield return new WaitForSeconds(playerMotion.switchItemSpeed); //Animation time
     
        playerMotion.switchQueue = false;
        //Can now shoot
    }
    //Move to player's back
    IEnumerator MoveToBack(Pickup pickup)
    {
        if (pickup.itemCollision.enabled)
        {
            pickup.DisableItemPhysics();
        }

        if (((primarySlot.isEquipSlot && primarySlot.item == null)          //Call discard if equip slot is empty
            || (secondarySlot.isEquipSlot && secondarySlot.item == null))
            && pickup.transform.parent != null)
        {
            playerMotion.onDiscard();
        }
        pickup.transform.parent = onBackTransform;
        pickup.transform.localPosition = new Vector3(0f, 0f, 0f);
        pickup.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        yield return new WaitForSeconds(playerMotion.switchItemSpeed); //Animation time
        playerMotion.switchQueue = false;
    }
    public void WeaponSwitchToHands(Pickup pickup)
    {
        playerMotion.switchQueue = true;
        //Put in timer for animation
        StartCoroutine(EquipItem(pickup));
    }

    public void WeaponSwitchToBack(Pickup pickup)
    {
        playerMotion.switchQueue = true;
        //Put in timer for animation
        StartCoroutine(MoveToBack(pickup));
    }
    void DropEquippedItem()
    {
        playerMotion.onDiscard();
        if (primarySlot.isEquipSlot && primarySlot.equippedPickup != null) 
        {
            primarySlot.equippedPickup.DropItem(primarySlot.item.inventoryItem);
        }
        else if (secondarySlot.isEquipSlot && secondarySlot.equippedPickup != null)
        {
            secondarySlot.equippedPickup.DropItem(secondarySlot.item.inventoryItem);
        }
    }

    void Update()
    {
        Keyboard k = InputSystem.GetDevice<Keyboard>();

        if (k.tabKey.isPressed && !inventoryEnabled) // enable afteer testing
        {
            inventoryEnabled = true;
            EnableInventory();
        }
        else if (!k.tabKey.isPressed && inventoryEnabled)
        {
            inventoryEnabled = false;
            DisableInventory();
        }

        if (k.digit1Key.isPressed)      //Primary control
        {
            if (!primarySlot.isEquipSlot && !playerMotion.switchQueue)
            {
                Debug.Log("Using primary slot");
                secondarySlot.isEquipSlot = false;
                primarySlot.isEquipSlot = true;

                primarySlot.equipStatusSet = false;
                secondarySlot.equipStatusSet = false;
            }
        }
        if (k.digit2Key.isPressed)      //Secondary control
        {
            if (!secondarySlot.isEquipSlot && !playerMotion.switchQueue)
            {
                Debug.Log("Using secondary slot");
                primarySlot.isEquipSlot = false;
                secondarySlot.isEquipSlot = true;

                //Change equip to false so we can check the item equip status
                primarySlot.equipStatusSet = false;
                secondarySlot.equipStatusSet = false;
            }
        }
        if (k.gKey.IsPressed() && !playerMotion.switchQueue)
        {
            DropEquippedItem();
        }

    }
}
