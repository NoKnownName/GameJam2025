using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioClip audioclip;
    [SerializeField] KeyCode inputkey = KeyCode.Alpha1;
    [SerializeField] bool stopOnRelease = true;
    private AudioSource audiosource;

    void Start()
    {
        audiosource = gameObject.AddComponent<AudioSource>();
        //audiosource.playOnAwake = false;
        audiosource.clip = audioclip;
    }

    void Update()
    {
        if (Input.GetKeyDown(inputkey))
        {
            if (audioclip != null)
            {
                audiosource.Stop();
                audiosource.Play();
            }
        }

        if (stopOnRelease && Input.GetKeyUp(inputkey))
        {
            audiosource.Stop();
        }
    }
}
