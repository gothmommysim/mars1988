using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CassettePlayer : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] cassetteTapes;
    [SerializeField] int trackNum;

    // Start is called before the first frame update
    void Start()
    {
        trackNum = 0;
        audioSource.loop = false;
        audioSource.clip = cassetteTapes[trackNum];
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying && trackNum<cassetteTapes.Length-1)
        {
            trackNum++;
            audioSource.clip = cassetteTapes[trackNum];
            audioSource.Play();
        }
        else if(!audioSource.isPlaying)
        {
            trackNum = 0;
        }
        
    }
}
