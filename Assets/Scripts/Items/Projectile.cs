using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public float bulletVelocity=10000f;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody projectilePhysics = GetComponent<Rigidbody>();

        projectilePhysics.AddForce(transform.up*Time.deltaTime*bulletVelocity);

    }
}
