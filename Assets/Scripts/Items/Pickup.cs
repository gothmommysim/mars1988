using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class Pickup : MonoBehaviour
{
    float throwOffset = 10000f;

    public bool itemPicked = false;

    public Item uniqueItem;

    public Transform rightHandTransform;
    public Transform adsTransform;

    [SerializeField] float clipRange = 0.62f;
    [SerializeField] float clipAngle = 10f;
    [SerializeField] float clipPos = 0.06f;
    [SerializeField] Transform clipTransform;

    [SerializeField] float clipXOffset = -.17f;
    [SerializeField] float clipYOffset = .24f;
    [SerializeField] float clipZOffset = -.33f;
    [SerializeField] float clipDistance;

    bool touchingSurface = false;
    bool gripsAssigned = false;
    //bool isShooting;
    bool allowShotQueue;
    bool setZoom = false;
    
    GameObject player;
    InventoryManager playerInventory;
    PlayerMovement playerMotion;
    Transform RHGrip;
    Transform LHGrip;

    public Transform pistolGrip;
    public Transform handGuard;

    public Collider itemCollision;
    Rigidbody itemPhysics;
    AudioSource itemAudio;
    void Awake()
    {
        allowShotQueue = true;
        //isShooting = false;
        TryGetComponent<Collider>(out itemCollision);
        TryGetComponent<Rigidbody>(out itemPhysics);
        TryGetComponent<AudioSource>(out itemAudio);
    }

    public void PassItemStats(Item item) //Pass in INSTANCES ONLY
    {
        uniqueItem = item;
        uniqueItem.instancedObj = gameObject;
    }

    public void DropItem(InventoryItem uiItem)
    {

        itemPicked = false;

        playerInventory.RemoveItem(uniqueItem);

        uniqueItem.inventoryItem = null;

        Object.Destroy(uiItem.gameObject);

        gripsAssigned = false;

        //Enable item physics and remove parent
        transform.parent = null;
        EnableItemPhysics();

        itemPhysics.AddForce(player.transform.forward * Time.deltaTime * throwOffset * playerMotion.playerStrength);
    }

    public void EnableItemPhysics()
    {
        itemCollision.enabled = true;
        itemPhysics.useGravity = true;
        itemPhysics.isKinematic = false;
    }
    public void DisableItemPhysics()
    {
        //Disable item physics
        itemCollision.enabled = false;
        itemPhysics.useGravity = false;
        itemPhysics.isKinematic = true;
    }

    public void MoveToInventory()
    {
        gameObject.SetActive(false);
    }
    public void EnablePlayerTransforms(GameObject currentUser)
    {
        player = currentUser;
        playerMotion = player.GetComponentInParent<PlayerMovement>();
        playerInventory = player.GetComponent<InventoryManager>();

        GameObject userCam = currentUser.transform.Find("handHint").GetChild(0).gameObject;

        rightHandTransform = userCam.transform.Find("Hand");
        adsTransform = rightHandTransform.GetChild(0);
        clipTransform = rightHandTransform;
    }

    public void ItemPicked(GameObject currentUser)
    {
        Debug.Log("Picked item"); Debug.Log(uniqueItem);
        EnablePlayerTransforms(currentUser);

        playerInventory.CheckInventoryFit(this);
    }

    void UpdateItemPosition()
    {
        //Draws ray to check for clips
        //*****************************************************
        //**CHANGE THIS TO SPHERE COLLIDER FOR BETTER RESULTS**
        //*****************************************************
        Ray clipCheck = new Ray(clipTransform.TransformPoint(
            new Vector3(clipTransform.localPosition.x + clipXOffset, clipTransform.localPosition.y + clipYOffset, clipTransform.localPosition.z + clipZOffset)),
            clipTransform.forward);
        RaycastHit envHit;

        Debug.DrawRay(clipTransform.TransformPoint(
            new Vector3(clipTransform.localPosition.x + clipXOffset, clipTransform.localPosition.y + clipYOffset, clipTransform.localPosition.z + clipZOffset)),
            clipTransform.forward * clipRange, Color.blue);
        
        if (Physics.Raycast(clipCheck, out envHit, clipRange))
        {
            if (envHit.collider.transform.root.tag == "Environment")
            {
                clipDistance = envHit.distance;
                touchingSurface = true;
            }
        }
        else
        {
            touchingSurface = false;
        }

        if (!touchingSurface)
        {
            if (playerMotion.isAiming && (uniqueItem is Weapon))
            {
                AimDownSights();
            }
            else if(playerMotion.isAiming && uniqueItem is Item &&uniqueItem.itemName.Contains("binoculars"))
            {
                if (!setZoom)
                {
                    playerMotion.virtualCamera.m_Lens.FieldOfView = 4f;
                    if (uniqueItem.instancedObj.GetComponentInChildren<MeshRenderer>())
                    {
                        uniqueItem.instancedObj.GetComponentInChildren<MeshRenderer>().enabled = false;
                        GameObject.Find("Player/UI/binoculars overlay").SetActive(true);
                        playerMotion.tempMouseSens = playerMotion.mouseSensitivity;
                        playerMotion.mouseSensitivity /= 2;
                    }
                    playerMotion.switchQueue = true;
                    setZoom = true;
                }
            }
            else
            {
                if(setZoom)
                {
                    playerMotion.virtualCamera.m_Lens.FieldOfView = 70f;
                    if (uniqueItem.instancedObj.GetComponentInChildren<MeshRenderer>())
                    {
                        uniqueItem.instancedObj.GetComponentInChildren<MeshRenderer>().enabled = true;
                        playerMotion.mouseSensitivity = playerMotion.tempMouseSens;   
                    }
                    GameObject.Find("Player/UI/binoculars overlay").SetActive(false);
                    playerMotion.switchQueue = false;
                    setZoom = false;
                }
                ItemFollowPlayer();
            }
        }
        else
        {
            WeaponUp();
        }

    }

    void Update()
    {

        if (transform.parent != null)
        {
            //If item is equipped
            if (playerInventory != null)
            {
                if (transform.parent.name.Equals(playerInventory.equipItemTransform.name))
                {
                    //Only assign grips on initial equip
                    if (!gripsAssigned)
                    {
                        RHGrip = transform.parent.Find("playerRightGrip");
                        LHGrip = transform.parent.Find("playerLeftGrip");
                        //RHGrip.GetComponent<MultiParentConstraint>().
                        // RHGrip.GetComponent<MultiParentConstraint>().data.sourceObjects.SetTransform(1, handGuard);

                        gripsAssigned = true;
                    }


                    RHGrip.position = pistolGrip.position;
                    RHGrip.rotation = pistolGrip.rotation;
                    LHGrip.position = handGuard.position;
                    LHGrip.rotation = handGuard.rotation;

                    UpdateItemPosition();

                    if (uniqueItem is Weapon)
                    {
                        SemiAutoFire();
                    }
                }
            }
        }
    }

    void Fire()
    {
        Debug.Log("shooting");
        itemAudio.Play();

        var weapon = uniqueItem as Weapon;
        var horizontalRecoilRandomVar = Random.Range(-weapon.horizontalRecoil, weapon.horizontalRecoil);
        transform.localPosition = new Vector3(transform.localPosition.x + horizontalRecoilRandomVar, transform.localPosition.y + weapon.verticalRecoil, transform.localPosition.z - weapon.horizontalRecoil);
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x - weapon.recoilAngle, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        playerMotion.camXRotation -= weapon.verticalRecoil * 100;
        playerMotion.camYRotation += horizontalRecoilRandomVar * 100;
    }

    IEnumerator FullAutoFire()
    {
        Fire();
        allowShotQueue = false; //can't add additional shots in queue while waiting
        var weapon = uniqueItem as Weapon;
        yield return new WaitForSeconds(1/(weapon.fireRate/60));
        allowShotQueue = true;
    }
    void SemiAutoFire()
    {
        Mouse m = InputSystem.GetDevice<Mouse>();

        if (m.leftButton.isPressed && allowShotQueue && playerMotion.allowFire)
        {
            StartCoroutine(FullAutoFire());
        }

        /*//Semi-auto code
        if (m.leftButton.isPressed && !isShooting)
        {
            Fire();
            isShooting = true;
        }else if (!m.leftButton.isPressed && isShooting)
        {
            isShooting = false;
        }*/
    }
    void WeaponUp()
    {

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler((rightHandTransform.rotation.eulerAngles.x - clipAngle / clipDistance), rightHandTransform.rotation.eulerAngles.y, rightHandTransform.rotation.eulerAngles.z),
                Time.deltaTime * playerMotion.playerGripSpeed);

        transform.position =
            Vector3.Lerp(
                transform.position,
                rightHandTransform.TransformPoint(new Vector3(rightHandTransform.localPosition.x, rightHandTransform.localPosition.y, Mathf.Clamp(rightHandTransform.localPosition.z - clipPos * 1 / clipDistance, -clipPos * 2, 0f))),
                Time.deltaTime * playerMotion.playerGripSpeed);
    }
    void ItemFollowPlayer()
    {
        if (clipTransform == null || clipTransform != rightHandTransform)
        {
            clipTransform = rightHandTransform;
        }
  
        //Item follows character transform
        transform.position = Vector3.Lerp(transform.position, rightHandTransform.position, Time.deltaTime * playerMotion.playerGripSpeed);


        //Item follows character rotation
        //
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerMotion.lookTransform.position - transform.position), Time.deltaTime * playerMotion.playerGripSpeed);
        //transform.rotation = rightHandTransform.rotation;
    }
    void AimDownSights()
    {
        if (clipTransform == rightHandTransform)
        {
            clipTransform = adsTransform;
        }
        //Item follows ads transform
        transform.position = Vector3.Lerp(transform.position, adsTransform.position, Time.deltaTime * playerMotion.playerGripSpeed);
        //transform.position = adsTransform.position;
        //Item follows ads rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, adsTransform.rotation, Time.deltaTime * playerMotion.playerGripSpeed);
        //transform.rotation = adsTransform.rotation;
    }
}
