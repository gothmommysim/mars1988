using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{

    //Semi-auto fire
    bool firing = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Pickup pu = gameObject.GetComponent<Pickup>();

        //If current weapon has been picked up
        if (pu.itemPicked)
        {
            Action();
        }
    }

    void Action()
    {
        Mouse m = InputSystem.GetDevice<Mouse>();
        Light flashlight = GetComponentInChildren<Light>();

        if (m.leftButton.IsPressed() && !firing && !flashlight.enabled)
        {
            firing = true;
            
            flashlight.enabled = true;
        }
        else if (m.leftButton.IsPressed() && !firing && flashlight.enabled)
        {
            firing = true;

            flashlight.enabled = false;
        }
        else if (!m.leftButton.IsPressed())
        {
            firing = false;
        }
    }
    }
