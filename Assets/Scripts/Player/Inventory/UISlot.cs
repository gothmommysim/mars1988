using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class UISlot : MonoBehaviour
{
    public int localX = 0;
    public int localY = 0;
    public Item item;
    public bool storageBuilt;
    public Storage slotContainer; //Storage that holds the actual slot

    void Update()
    {

    }
}
