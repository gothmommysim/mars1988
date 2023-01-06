using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemManager
{

    public static class Create
    {
        public static Pickup BuildPhysicalItem(Transform spawn, Item item)
        {
            item.instancedObj = Object.Instantiate(item.baseObj, spawn);
            var instancedPickup = item.instancedObj.GetComponent<Pickup>();
            instancedPickup.PassItemStats(item);

            return instancedPickup;
        }
        public static Item BuildItem(Item item)
        {
            if (item is Storage)
            {
                var tempStorage = BuildStorage(Object.Instantiate(item) as Storage);
                return tempStorage;
            }
            else
            {
                return Object.Instantiate(item);
            }
        }
        public static Storage BuildStorage(Storage storage)
        {
            if (storage.storageSlots != null) return storage;
            
            storage.storageSlots = new Slot[storage.storageWidth, storage.storageHeight];
            for (var y = 0; y < storage.storageHeight; y++)
            {
                for (var x = 0; x < storage.storageWidth; x++)
                {
                    storage.storageSlots[x, y] = new Slot
                    {
                        slotContainer = storage,
                        localX = x,
                        localY = y,
                        slotFilled = false
                    };
                }
            }

            return storage;
        }
    }
}
