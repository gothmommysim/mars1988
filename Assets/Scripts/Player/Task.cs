using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New Task", menuName = "Task")]
public class Task : ScriptableObject
{
    public string taskDescripton = "";
    public string originalTaskDesc = "";
    public Task nextTask;
    public bool taskComplete;

    [System.Serializable]
    public struct CurrentTask{
        public string dialogueGiverName;
        public GameObject objectInteract;
        public string taskDescUpdate;
        public PlayAudio playAudio;
        public ObserveObject observeObject;
        public ActivateObject activateObject;
        [TextArea(3, 3)]
        public List<string> dialogueList;
        public List<DialogueOption> dialogueOptionsList;
    }

    [System.Serializable]
    public struct DialogueOption{
        public string dialogueOption;
        [TextArea(3, 3)]
        public List<string> dialogueList;
        public bool goToNext;
        public bool giveTaskComplete;
    }

    [System.Serializable]
    public struct PlayAudio
    {
        public GameObject audioSource;
        public AudioClip audioClip;
        public bool loop;
    }

    [System.Serializable]
    public struct ObserveObject
    {
        public string objectName;
        public float range;
    }

    [System.Serializable]
    public struct ActivateObject
    {
        public string objectName;
        public bool deactivate;
    }

    public List<CurrentTask> currentTasks = new List<CurrentTask>();
}
