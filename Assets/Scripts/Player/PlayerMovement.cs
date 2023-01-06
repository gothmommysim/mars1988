using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject playerModel;
    public CinemachineVirtualCamera virtualCamera;

    public AvatarMask playerDefaultMask;
    public AvatarMask playerHandsMask;

    public Animator playerAnim;

    public float baseSpeed = 3.3f; //(3.3) base
    public float terminalSpeed = 5f;
    public float acceleration = 0.1f;
    public float gravy = -9.81f; //(gravy*5)
    public float jumpHeight = 2f;
    public float jumpDelay = 1f;
    public float playerStrength
    {
        get { return _playerStrength; }
        set { _playerStrength = Mathf.Clamp(value, 0, 1); }
    }
    [SerializeField, Range(0, 100)] private float _playerStrength = 0.1f;

    public TwoBoneIKConstraint RightHandIK;
    public TwoBoneIKConstraint LeftHandIK;
    public MultiAimConstraint HipIK;
    public bool isAiming = false;
    //bool isSprinting = false;
    bool isLeaning = false;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Vector3 velocity;
    float xSpeed = 0;
    float zSpeed = 0;
    float x = 0;
    float z = 0;
    bool onGround;

    bool canJump = true;
    bool leanEnabled = false;

    float animationSpeed = 15f;

    //Leaning vars
    public float leanSpeed = 100f;
    public float maxLean = 25f;
    public float positionChange = 2f;

    [Header("Weapon Settings")]
    public float playerGripSpeed = 14f;
    //Weapon switch speed
    public float switchItemSpeed = 0f;
    //Are weapons switching?
    public bool switchQueue = false;
    public bool allowFire = true;
    public bool allowMovement = true;

    [Header("Camera Control")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform handTransform;
    public Transform lookTransform;
    bool updateRotate;
    public bool hasCameraControl;

    public float mouseSensitivity = 100f;
    public float tempMouseSens;

    List<float> cameraCoordinates = new List<float>();

    public float camXRotation = 0f;
    public float camYRotation = 0f;
    float camZRotation = 0f;
    float camYPosition = 0f;
    float camXPosition = 0f;
    float camZPosition = 0f;
    float handYPosition
    {
        get { return _handYPosition; }
        set { _handYPosition = Mathf.Clamp(value, minReach, maxReach); }
    }
    [SerializeField] readonly float maxReach = 1.7f;
    [SerializeField] readonly float minReach = .55f;
    float _handYPosition = 0f;

    void Awake()
    {
        switchItemSpeed = 0f;
        handYPosition = maxReach-minReach;
        hasCameraControl = true;
        updateRotate = true;
        cameraCoordinates = new List<float>() {
        new float(),
        new float(),
        new float(),
        new float(),
        new float(),
        new float()
    };
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Start()
    {
        //Disabled chest for now lul
        playerDefaultMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, false);
        //

        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, false);
        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, false);

        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, false);
        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, false);
    }

    void CameraControl()
    {
        if (hasCameraControl)
        {
            Keyboard k = InputSystem.GetDevice<Keyboard>();
            Mouse m = InputSystem.GetDevice<Mouse>();

            float mouseX = m.delta.x.ReadValue() * mouseSensitivity * Time.deltaTime;
            float mouseY = m.delta.y.ReadValue() * mouseSensitivity * Time.deltaTime;

            
            camXRotation -= mouseY;
            camXRotation = Mathf.Clamp(camXRotation, -80f, 80f);
            //handYPosition += mouseY / 160f; // 160 is complete area of clamp (from -80 to 80)

            // Alt - Look
            if (k.altKey.IsPressed())
            {
                updateRotate = false;
            }
            else if (!k.altKey.IsPressed() && !updateRotate)
            {
                camYRotation = 0;

                updateRotate = true;
            }
            if (updateRotate)
            {
                transform.Rotate(Vector3.up * mouseX); // rotate player obj along y
            }
            else
            {
                camYRotation += mouseX;
                camYRotation = Mathf.Clamp(camYRotation, -70f, 70f);
            }

            //Passing coordinates into Player Movement
            cameraCoordinates[0] = camXPosition;
            cameraCoordinates[1] = camYPosition;
            cameraCoordinates[2] = camZPosition;
            cameraCoordinates[3] = camXRotation;
            cameraCoordinates[4] = camYRotation;
            cameraCoordinates[5] = camZRotation;

            //Leaning follows camera
            Leaning(k);
            camXPosition = cameraCoordinates[0];
            camZRotation = cameraCoordinates[5];

            cameraTransform.localRotation = Quaternion.Euler(camXRotation, camYRotation, camZRotation);
            cameraTransform.localPosition = new Vector3(camXPosition, cameraTransform.localPosition.y, cameraTransform.localPosition.z);
            //handTransform.rotation = cameraTransform.rotation;
            //handTransform.localPosition = new Vector3(handTransform.localPosition.x, handYPosition, handTransform.localPosition.z);
        }
    }

    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(jumpDelay);

        canJump = true;
    }

    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravy);
        canJump = false;

        StartCoroutine(JumpDelay());
    }

    //Slows the player to zero
    void StopZMoving()
    {
        if (zSpeed > 0.1)
        {
            zSpeed -= acceleration;
        }
        else
        {
            zSpeed = 0;
        }

        if (zSpeed < 0.1)
        {
            z = 0;
        }
    }

    void StopXMoving()
    {
        if (xSpeed > 0.1)
        {
            xSpeed -= acceleration;
        }
        else
        {
            xSpeed = 0;
        }

        if (xSpeed < 0.1)
        {
            x = 0;
        }
    }

    float MoveLeft(float x)
    {
        playerAnim.SetFloat("walk_direction", Mathf.SmoothStep(playerAnim.GetFloat("walk_direction"), -1f, animationSpeed * Time.deltaTime));

        x = -1f;
        if (xSpeed < baseSpeed)
        {
            xSpeed += acceleration;
        }
        return x;
    }
    float MoveRight(float x)
    {
        playerAnim.SetFloat("walk_direction", Mathf.SmoothStep(playerAnim.GetFloat("walk_direction"), 1f, animationSpeed * Time.deltaTime));
        x = 1f;
        if (xSpeed < baseSpeed)
        {
            xSpeed += acceleration;
        }
        return x;
    }

    float MoveForward(float z)
    {
        //Change animation set
        playerAnim.SetFloat("walk_speed", 1.0f);

        z = 1f;
        if (zSpeed < baseSpeed)
        {
            zSpeed += acceleration;
        }
        return z;
    }
    float MoveReverse(float z)
    {
        playerAnim.SetFloat("walk_speed", Mathf.SmoothStep(playerAnim.GetFloat("walk_speed"), -1f, animationSpeed * Time.deltaTime));
        z = -1f;
        if (zSpeed < baseSpeed)
        {
            zSpeed += acceleration;
        }
        return z;
    }

    public void onPickup()
    {
        //Enable IK for hands on item
        //RightHandIK = transform.Find("RightHandIK").GetComponent<TwoBoneIKConstraint>();
        //LeftHandIK = transform.Find("LeftHandIK").GetComponent<TwoBoneIKConstraint>();

        RightHandIK.weight = 1;
        LeftHandIK.weight = 1;

        //Animate fingers in weapon holding position
        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);

        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
    }

    public void onDiscard()
    {
        //Disable IK for hands on item
        RightHandIK.weight = 0;
        LeftHandIK.weight = 0;

        //Reanimate fingers
        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, false);
        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, false);

        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, false);
        playerHandsMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, false);
    }

    public void Leaning(Keyboard k)
    {


        var xPosition = cameraCoordinates[0];
        var zRotation = cameraCoordinates[5];

        var leanRatio = Mathf.Abs(zRotation / maxLean);
        HipIK.weight = leanRatio;

        if (leanEnabled && k.qKey.IsPressed())
        {
            isLeaning = true;

            //Disable chest animations while leaning
            playerDefaultMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, false);
            if (zRotation < maxLean)
            {
                zRotation += leanSpeed * Time.deltaTime;
                xPosition -= positionChange * Time.deltaTime;
            }
        }
        else if (leanEnabled && k.eKey.IsPressed())
        {
            isLeaning = true;
            playerDefaultMask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, false);
            if (zRotation > -maxLean)
            {
                zRotation -= leanSpeed * Time.deltaTime;
                xPosition += positionChange * Time.deltaTime;
            }
        }
        else if (zRotation != 0)
        {
            if (zRotation < 0)
            {
                zRotation += leanSpeed * Time.deltaTime;
                xPosition -= positionChange * Time.deltaTime;
            }
            else if (zRotation > 0)
            {
                zRotation -= leanSpeed * Time.deltaTime;
                xPosition += positionChange * Time.deltaTime;
            }
            if (Mathf.Abs(zRotation) < 1)
            {
                zRotation = 0f;
                xPosition = 0f;
                isLeaning = false;
            }
        }

        cameraCoordinates[0] = xPosition;
        cameraCoordinates[5] = zRotation;

        //return cameraCoordinates;
    }

    // Update is called once per frame
    void Update()
    {
        CameraControl();

        Keyboard k = InputSystem.GetDevice<Keyboard>();
        Mouse m = InputSystem.GetDevice<Mouse>();

        //Check Ground Collision
        onGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        //Fall
        if (onGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //Key bind to movement
        //Move forward
        if (k.wKey.IsPressed() && allowMovement)
        {
            if (k.sKey.IsPressed())
            {
                StopZMoving();
            }
            else
            {
                z = MoveForward(z);
            }
        }
        //Move backward
        if (k.sKey.IsPressed() && allowMovement)
        {
            if (k.wKey.IsPressed())
            {
                StopZMoving();
            }
            else
            {
                z = MoveReverse(z);
            }
        }
        //Move left
        if (k.aKey.IsPressed() && allowMovement)
        {
            if (x == 1f && xSpeed > 0.1)
            {
                StopXMoving();
            }
            else
            {
                x = MoveLeft(x);
            }
        }
        //Move right
        if (k.dKey.IsPressed() && allowMovement)
        {
            if (x == -1f && xSpeed > 0.1)
            {
                StopXMoving();
            }
            else
            {
                x = MoveRight(x);
            }
        }
        //Stop movement
        if (!k.wKey.IsPressed() && !k.sKey.IsPressed() && !k.aKey.IsPressed() && !k.dKey.IsPressed())
        {
            StopZMoving();
            StopXMoving();
            playerAnim.SetFloat("walk_speed", 0.0f);
        }
        if (!(k.aKey.IsPressed() || k.dKey.IsPressed()))
        {
            playerAnim.SetFloat("walk_direction", Mathf.SmoothStep(playerAnim.GetFloat("walk_direction"), 0f, animationSpeed * Time.deltaTime));
            StopXMoving();
        }
        if (!(k.wKey.IsPressed() || k.sKey.IsPressed()))
        {
            playerAnim.SetFloat("walk_direction", Mathf.SmoothStep(playerAnim.GetFloat("walk_direction"), 0f, animationSpeed * Time.deltaTime));
            StopZMoving();
        }

        //Sprint
        if (k.leftShiftKey.IsPressed() && !isAiming && !isLeaning && zSpeed > 0.1)
        {
            //Change to be affected when facing a wall // change to float (z_movement)
            playerAnim.SetFloat("run_speed", 1f);
            //isSprinting = true;
            if (zSpeed < terminalSpeed)
            {
                zSpeed += acceleration;
            }
        }
        else
        {
            playerAnim.SetFloat("run_speed", 0f);
            //isSprinting = false;
            if (zSpeed > baseSpeed)
            {
                zSpeed -= acceleration;
            }
        }


        if (m.rightButton.IsPressed())
        {
            isAiming = true;
            
        }
        else
        {
            isAiming = false;
        }


        if (canJump)
        {
            if (onGround && k.spaceKey.IsPressed())
            {
                Jump();
            }
        }



        //Horizontal movement
        Vector3 move = transform.right * x * xSpeed + transform.forward * z * zSpeed;

        controller.Move(move * Time.deltaTime);

        //Vertical movement
        velocity.y += gravy * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);


    }
}
