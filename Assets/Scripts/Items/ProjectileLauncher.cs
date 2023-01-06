using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileLauncher : MonoBehaviour
{
    int ammoCount = 7;
    public GameObject projectile;
    public AudioClip fireAudio;
    public AudioClip outOfAmmoAudio;

    public Transform projectileSpawnLocation;

    public AudioSource weaponAudio;

    WaitForSeconds shotTimer = new WaitForSeconds(.05f);

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

        if (m.leftButton.IsPressed() && !firing && ammoCount > 0)
        {
            firing = true;
            weaponAudio.clip = fireAudio;
            weaponAudio.Play();
            ammoCount--;

            GameObject currentBullet = Instantiate(projectile);
            currentBullet.transform.position = projectileSpawnLocation.position;
            currentBullet.transform.rotation = Quaternion.Euler(projectileSpawnLocation.rotation.eulerAngles.x + 90f,
                projectileSpawnLocation.rotation.eulerAngles.y, projectileSpawnLocation.rotation.eulerAngles.z);
        }
        else if (!m.leftButton.IsPressed())
        {
            firing = false;
        }
        else if (m.leftButton.IsPressed() && !firing && ammoCount < 1)
        {
            firing = true;
            weaponAudio.clip = outOfAmmoAudio;
            weaponAudio.Play();
        }
    }
}
