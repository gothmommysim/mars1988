using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageSpawner : MonoBehaviour
{
    public Weapon weapon;
    //
    GameObject wepObj;
    GameObject physicalWep;

    // Start is called before the first frame update
    void Start()
    {
        wepObj = weapon.baseObj;
        physicalWep = Instantiate(wepObj, transform);
        physicalWep.GetComponent<Pickup>().PassItemStats(Object.Instantiate(weapon));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
