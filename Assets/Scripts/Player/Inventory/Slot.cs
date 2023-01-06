using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class Slot
{
    public int localX = 0;
    public int localY = 0;
    public Item item;
    public bool slotFilled = false;
    public Storage slotContainer; //Storage that holds the slot
}
