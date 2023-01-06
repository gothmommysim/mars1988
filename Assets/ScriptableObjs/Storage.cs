using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Storage", menuName = "Items/Storage")]
public class Storage : Item
{
    public int storageWidth = 0;
    public int storageHeight = 0;
    public Slot[,] storageSlots;
    public enum StorageTypes { itemStorage, equipStorage, storeStorage}
    public StorageTypes storageType;
}