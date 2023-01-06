using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Spawner : MonoBehaviour
{
    public GameObject Player;

    public Transform[] spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        //replace with created points in map
        spawnPoint[0] = transform;

        Instantiate(Player, spawnPoint[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
