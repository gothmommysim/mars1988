// On camTransform object

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRayInteract : MonoBehaviour
{

    public float range = 100f;
    
    public Transform cameraTransform;

    public GameObject interactIndicator;

    public RaycastHit itemHit;
    public Ray whereLook;

    bool inventoryEmpty = true;

    //Boolean to tell if UI has updated
    bool UIHasUpdated = false;

    [SerializeField] TaskSystem taskSystem;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        whereLook = new Ray(cameraTransform.position, cameraTransform.forward);

        Keyboard k = InputSystem.GetDevice<Keyboard>();

        Debug.DrawRay(cameraTransform.position,
            cameraTransform.forward * range, Color.blue);
        if (Physics.Raycast(whereLook, out itemHit, range))
        {
            if (itemHit.collider.tag == "Item")
            {
                Pickup item = itemHit.transform.GetComponent<Pickup>();
                //Only update the UI if it hasn't since looking at it.
                if (!UIHasUpdated)
                {
                    UpdateUI();
                }

                //On "Interact" button press
                if (k.fKey.IsPressed() && !item.itemPicked)
                {
                    interactIndicator.SetActive(false);

                    if (item != null)
                    {
                        //Check if we can place the item in our inventory ****************************************
                        item.ItemPicked(transform.root.gameObject);
                        //inventoryEmpty = false;
                    }
                }
                if (taskSystem.currentTask.currentTasks[TaskSystem.taskId].objectInteract != null && 
                    itemHit.collider.name.Contains(taskSystem.currentTask.currentTasks[TaskSystem.taskId].objectInteract.name) &&
                    k.fKey.IsPressed() && !taskSystem.activateInteractable)
                {
                    taskSystem.activateInteractable = true;
                }
            }
            else if (taskSystem.currentTask.currentTasks[TaskSystem.taskId].objectInteract != null && 
                itemHit.collider.name.Contains(taskSystem.currentTask.currentTasks[TaskSystem.taskId].objectInteract.name))
            {
                if (!UIHasUpdated)
                {
                    UpdateUI();
                }

                if (k.fKey.IsPressed() && !taskSystem.activateInteractable)
                {
                    taskSystem.activateInteractable = true;
                }
            }
            else
            {
                //Disable crosshair
                interactIndicator.SetActive(false);
                UIHasUpdated = false;
            }
        }
        else
        {
            //Disable crosshair
            interactIndicator.SetActive(false);
            UIHasUpdated = false;
        }

        if (k.gKey.IsPressed() && !inventoryEmpty)
        {
            inventoryEmpty = true;
        }
    }

    void UpdateUI()
    {
        //Show crosshair
        interactIndicator.SetActive(true);
        UIHasUpdated = true;
    }
 }

