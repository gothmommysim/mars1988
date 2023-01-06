// On TaskSystem object

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TaskSystem : MonoBehaviour
{
    public PlayerMovement playerMotion;

    [SerializeField] TMPro.TextMeshProUGUI taskDescText;
    [SerializeField] TMPro.TextMeshProUGUI dialogueGiverText;
    [SerializeField] TMPro.TextMeshProUGUI dialogueText;
    [SerializeField] GameObject dialogueBoxObj;
    [SerializeField] GameObject dialogueIndicatorObj;
    [SerializeField] GameObject dialogueOptionsObj;
    [SerializeField] GameObject dialogueOptionButton;
    [SerializeField] GameObject dialogueOptionContinueButton;
    [SerializeField] public Task currentTask;
    [SerializeField] AudioSource dialogueSpeaker;
    [SerializeField] AudioClip[] audioVowels;
    [SerializeField] AudioSource audioComponent;
    [SerializeField] CameraRayInteract cameraRayInteract;
    public bool activateInteractable = false;
    bool audioPlayed = false;
    bool objectActivated = false;

    List<string> currentDialogueList;

    //Setting new task
    bool descSet = false;
    public static int taskId = 0;
    int dialogueIndex = 0;
    bool dialogueRead = false;
    bool dialogueDone = false;
    bool dialogueOptionsActive = false;
    public static bool startDialogueOption = false;

    string periwinkleHex = "<color=#ccccff>";
    string neonGreenHex = "<color=#b4ff00>";
    string redHex = "<color=#ff0000>";
    string chrimaGreenHex = "<color=#34a65f>";
    string endColorHex = "</color>";
    float textSpeed = 0.05f;

    private void Start()
    {
        audioPlayed = false;
        objectActivated = false;
        audioComponent = null;
        currentTask.taskDescripton = currentTask.originalTaskDesc;
        currentDialogueList = currentTask.currentTasks[taskId].dialogueList;
        currentTask.taskComplete = false;
        dialogueBoxObj.SetActive(false);
        dialogueOptionsObj.SetActive(false);
        dialogueIndicatorObj.SetActive(false);
    }

    private void Update()
    {
        Keyboard k = InputSystem.GetDevice<Keyboard>();

        if (currentTask.taskComplete && !dialogueOptionsObj.activeSelf) //Check task complete
        {
            //Reset vars
            taskId = 0;
            dialogueIndex = 0;
            dialogueDone = false;
            dialogueRead = false;
            dialogueBoxObj.SetActive(false);
            dialogueGiverText.text = "";
            dialogueText.text = "";

            //Unset description
            descSet = false;

            dialogueOptionsActive = false;
            dialogueOptionsObj.SetActive(false);

            currentTask = currentTask.nextTask;
            currentTask.taskComplete = false;
        }

        if(taskId == 0 && currentTask.taskDescripton != currentTask.originalTaskDesc)
        {
            currentTask.taskDescripton = currentTask.originalTaskDesc;
        }

        //Task (Set new description)
        if (currentTask.currentTasks[taskId].taskDescUpdate != null && currentTask.currentTasks[taskId].taskDescUpdate != "")
        {
            currentTask.taskDescripton = currentTask.currentTasks[taskId].taskDescUpdate;
            descSet = false;
        }

        if (!descSet)
        {
            taskDescText.text = "> " + currentTask.taskDescripton;
            descSet = true;
        }

        //Task (Observe object)
        if (currentTask.currentTasks[taskId].observeObject.objectName != null && currentTask.currentTasks[taskId].observeObject.objectName != "")
        {
            RaycastHit itemHit = cameraRayInteract.itemHit;
            Ray whereLook = cameraRayInteract.whereLook;

            //with binoculars or some kind of aiming reticle use (playermotion.isaiming)
            if (playerMotion.isAiming && Physics.Raycast(whereLook, out itemHit) && itemHit.collider.name.Contains(currentTask.currentTasks[taskId].observeObject.objectName))
            {
                taskId++;
                objectActivated = false;
                audioPlayed = false;
            }
        }

        //Task (Activate object)
        if (currentTask.currentTasks[taskId].activateObject.objectName != null &&
            currentTask.currentTasks[taskId].activateObject.objectName != "" && !objectActivated)
        {
            if (!currentTask.currentTasks[taskId].activateObject.deactivate)
            {
                Debug.Log("the task id is:" + taskId);
                Debug.Log("the dialogue index is:" + dialogueIndex);
                Debug.Log(currentTask.currentTasks[taskId].activateObject.objectName);
                if (currentTask.currentTasks[taskId].activateObject.objectName.Contains("Spawner"))
                {
                    GameObject.Find(currentTask.currentTasks[taskId].activateObject.objectName).GetComponent<ItemSpawner>().enabled = true;

                }
                GameObject.Find(currentTask.currentTasks[taskId].activateObject.objectName).SetActive(true);
                objectActivated = true;
            }
            else
            {
                if (currentTask.currentTasks[taskId].activateObject.objectName.Contains("Spawner"))
                {
                    GameObject.Find(currentTask.currentTasks[taskId].activateObject.objectName).GetComponent<ItemSpawner>().enabled = false;
                }
                GameObject.Find(currentTask.currentTasks[taskId].activateObject.objectName).SetActive(false);
                objectActivated = true;
            }
        }

            //Task (play audio)
        if (currentTask.currentTasks[taskId].playAudio.audioClip != null && 
            currentTask.currentTasks[taskId].playAudio.audioClip.name != "" && !audioPlayed)
        {
            
            audioComponent = GameObject.Find(currentTask.currentTasks[taskId].playAudio.audioSource.name).GetComponentInChildren<AudioSource>();
            if (!audioComponent)
            {
                audioComponent = currentTask.currentTasks[taskId].playAudio.audioSource.AddComponent<AudioSource>();
                audioComponent.spatialBlend = 1;
            }
            if (!audioComponent.isPlaying)
            {
                audioComponent.clip = currentTask.currentTasks[taskId].playAudio.audioClip;
                audioComponent.loop = currentTask.currentTasks[taskId].playAudio.loop;
                audioComponent.Play();
                audioPlayed = true;
            }
        }
        else if(audioComponent.loop && !audioPlayed)
        {
            audioComponent.loop = false;
            audioComponent.Stop();
        }
        
        if (activateInteractable)
        {
            taskId++;
            objectActivated = false;
            audioPlayed = false;

            activateInteractable = false;
        }
        if (!dialogueRead && (currentTask.currentTasks[taskId].dialogueGiverName != null && currentTask.currentTasks[taskId].dialogueGiverName != ""))
        {
            //UnlockCursor();
            dialogueIndex = 0;
            dialogueDone = false;
            currentDialogueList = currentTask.currentTasks[taskId].dialogueList;
            StartCoroutine(GoThroughDialogue(currentDialogueList));
        }
        else if(!dialogueOptionsActive && (dialogueIndex + 1 < currentDialogueList.Count) && k.fKey.IsPressed() && dialogueDone)
        {
            dialogueDone = false;
            dialogueIndicatorObj.SetActive(false);
            dialogueBoxObj.SetActive(false);
            dialogueGiverText.text = "";
            dialogueText.text = "";

            dialogueIndex++;
            StartCoroutine(GoThroughDialogue(currentDialogueList));
        }
        else if (!dialogueOptionsActive && k.fKey.IsPressed() && dialogueDone)
        {
            dialogueIndex = 0;

            dialogueDone = false;
            dialogueRead = false;
            dialogueIndicatorObj.SetActive(false);
            dialogueBoxObj.SetActive(false);
            dialogueGiverText.text = "";
            dialogueText.text = "";

            taskId++;
            objectActivated = false;
            audioPlayed = false;
        }
        if (!dialogueOptionsActive && currentTask.currentTasks[taskId].dialogueOptionsList.Count > 0 && 
            (dialogueIndex == currentDialogueList.Count-1) && dialogueDone)
        {
            SetupDialogueOptions();
        }
        if (startDialogueOption)
        {
            startDialogueOption = false;
            GameObject currentObj = EventSystem.current.currentSelectedGameObject;
            TextMeshProUGUI textUI = currentObj.GetComponentInChildren(typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
            foreach (Task.DialogueOption dialogueOptionType in currentTask.currentTasks[taskId].dialogueOptionsList)
            {
                if (textUI.text == dialogueOptionType.dialogueOption)
                {
                    if (!dialogueOptionType.goToNext)
                    {
                        LockCursor();
                        dialogueDone = false;
                        dialogueRead = false;
                        dialogueBoxObj.SetActive(false);
                        dialogueGiverText.text = "";
                        dialogueText.text = "";

                        dialogueOptionsActive = false;
                        dialogueOptionsObj.SetActive(false);

                        currentDialogueList = dialogueOptionType.dialogueList;
                        StartCoroutine(GoThroughDialogue(currentDialogueList));
                        for (int i = 2; i < currentObj.transform.parent.transform.childCount; i++)
                        {
                            Object.Destroy(currentObj.transform.parent.transform.GetChild(i).gameObject);
                        }
                        return;
                    }
                    else
                    {
                        LockCursor();
                        dialogueIndex = 0;

                        dialogueDone = false;
                        dialogueRead = false;
                        dialogueBoxObj.SetActive(false);
                        dialogueGiverText.text = "";
                        dialogueText.text = "";

                        dialogueOptionsActive = false;
                        dialogueOptionsObj.SetActive(false);

                        taskId++; // make task id++ function
                        objectActivated = false;
                        audioPlayed = false;

                        Debug.Log(taskId);
                        for (int i = 2; i < currentObj.transform.parent.transform.childCount; i++)
                        {
                            Object.Destroy(currentObj.transform.parent.transform.GetChild(i).gameObject);
                        }
                        return;
                    }
                }
            }
        }
    }

    public static void StartDialogueOption()
    {
        startDialogueOption = true;
    }

    IEnumerator GoThroughDialogue(List<string> dialogueList){
        dialogueRead = true;
        dialogueBoxObj.SetActive(true);
        dialogueGiverText.text = periwinkleHex + currentTask.currentTasks[taskId].dialogueGiverName + endColorHex + ":";

        for(int i = 0; i < dialogueList[dialogueIndex].Length; i++)
        {
            dialogueSpeaker.loop = false;
            
            if (currentTask.currentTasks[taskId].dialogueGiverName.Contains("sim")){
                dialogueSpeaker.pitch = Random.Range(2.5f, 3f);
            }
            else
            {
                dialogueSpeaker.pitch = Random.Range(0.5f, 1.5f);
            }

            if(dialogueList[dialogueIndex][i] == 'a')
            {
                dialogueSpeaker.clip = audioVowels[0];
                dialogueSpeaker.Play();
            }
            if (dialogueList[dialogueIndex][i] == 'e')
            {
                dialogueSpeaker.clip = audioVowels[1];
                dialogueSpeaker.Play();
            }
            if (dialogueList[dialogueIndex][i] == 'i')
            {
                dialogueSpeaker.clip = audioVowels[2];
                dialogueSpeaker.Play();
            }
            if (dialogueList[dialogueIndex][i] == 'o')
            {
                dialogueSpeaker.clip = audioVowels[3];
                dialogueSpeaker.Play();
            }
            if (dialogueList[dialogueIndex][i] == 'u')
            {
                dialogueSpeaker.clip = audioVowels[4];
                dialogueSpeaker.Play();
            }


            //if string contains periwinkle
            if (dialogueList[dialogueIndex].Contains(periwinkleHex) && 
                i == dialogueList[dialogueIndex].IndexOf(periwinkleHex,i))
            {
                dialogueText.text += periwinkleHex;
                i += periwinkleHex.Length;
            }
            //if string contains neon green
            if (dialogueList[dialogueIndex].Contains(neonGreenHex) &&
                i == dialogueList[dialogueIndex].IndexOf(neonGreenHex,i))
            {
                dialogueText.text += neonGreenHex;
                i += neonGreenHex.Length;
            }
            //if string contains red
            if (dialogueList[dialogueIndex].Contains(redHex) &&
                i == dialogueList[dialogueIndex].IndexOf(redHex, i))
            {
                dialogueText.text += redHex;
                i += redHex.Length;
            }
            //if string contains chrima green
            if (dialogueList[dialogueIndex].Contains(chrimaGreenHex) &&
                i == dialogueList[dialogueIndex].IndexOf(chrimaGreenHex, i))
            {
                dialogueText.text += chrimaGreenHex;
                i += chrimaGreenHex.Length;
            }
            //if string contains color end code
            if (dialogueList[dialogueIndex].Contains(endColorHex) &&
                i == dialogueList[dialogueIndex].IndexOf(endColorHex,i))
            {
                dialogueText.text += endColorHex;
                i += endColorHex.Length;
            }
            if (i < dialogueList[dialogueIndex].Length) {
                dialogueText.text += dialogueList[dialogueIndex][i];
            }
            yield return new WaitForSeconds(textSpeed);
        }
        dialogueSpeaker.pitch = 1f;
        dialogueIndicatorObj.SetActive(true);
        dialogueDone = true;
    }

    void SetupDialogueOptions()
    {
        dialogueIndex = 0;
        dialogueIndicatorObj.SetActive(false);
        dialogueOptionsActive = true;
        dialogueOptionsObj.SetActive(true);
        UnlockCursor();

        for(int i = 0; i < currentTask.currentTasks[taskId].dialogueOptionsList.Count; i++)
        {
            if (!currentTask.currentTasks[taskId].dialogueOptionsList[i].goToNext)
            {
                GameObject newDialogueOptionObj = Instantiate(dialogueOptionButton, dialogueOptionButton.transform.parent);
                TextMeshProUGUI textUI = newDialogueOptionObj.GetComponentInChildren(typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                textUI.text = currentTask.currentTasks[taskId].dialogueOptionsList[i].dialogueOption;
                newDialogueOptionObj.SetActive(true);
            }
            else if (currentTask.currentTasks[taskId].dialogueOptionsList[i].goToNext)
            {
                GameObject newDialogueOptionObj = Instantiate(dialogueOptionContinueButton, dialogueOptionContinueButton.transform.parent);
                TextMeshProUGUI textUI = newDialogueOptionObj.GetComponentInChildren(typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                textUI.text = currentTask.currentTasks[taskId].dialogueOptionsList[i].dialogueOption;
                newDialogueOptionObj.SetActive(true);

                if (currentTask.currentTasks[taskId].dialogueOptionsList[i].giveTaskComplete)
                {
                    currentTask.taskComplete = true;
                }
            }
        }
    }
    void UnlockCursor()
    {
        playerMotion.hasCameraControl = false;
        playerMotion.allowFire = false;
        playerMotion.allowMovement = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    void LockCursor()
    {
        playerMotion.hasCameraControl = true;
        playerMotion.allowFire = true;
        playerMotion.allowMovement = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
