using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] music;
    public AudioSource source;

    public void Start()
    {
        source.Play();
    }

    public void ChangeToIce()
    {
        source.Stop();
        source.clip = music[1];
        source.Play();
    }

    public void ChangeToLava()
    {
        source.Stop();
        source.clip = music[2];
        source.Play();
    }
}
