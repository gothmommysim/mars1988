using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemManager;

public class ItemSpawner : MonoBehaviour
{
    public Item item;

    void Start()
    {
        for (int i = 0; i < 1; i++)
        {
            //Spawn a new item
            var newPickup = Create.BuildPhysicalItem(transform, Create.BuildItem(item));
            //Remove parent
            newPickup.transform.parent = null;
        }
    }

    void Update()
    {
        //Allow random generation of an item over time
        //Or only spawn items once players get close to reduce bog on the server
    }
}
