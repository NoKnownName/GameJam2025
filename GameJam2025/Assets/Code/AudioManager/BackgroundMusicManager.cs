using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public AudioClip backgroundMusic;
    public KeyCode muteKey = KeyCode.Space;
    public AudioSource musicSource;
    public bool isPlaying = true;

    void Start()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.Play();
    }

    void Update()
    {
        if (musicSource.isPlaying == false && Input.GetKeyDown(muteKey))
        {
            musicSource.Play();
        }
        else if(musicSource.isPlaying == true && Input.GetKeyDown(muteKey))
        {
            musicSource.Pause();
        }
    }
}
