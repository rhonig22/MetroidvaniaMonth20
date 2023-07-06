using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip currentClip;
    [SerializeField] public AudioClip ruins;
    [SerializeField] public AudioClip cave;
    [SerializeField] public AudioClip air;
    [SerializeField] public AudioClip finale;
    private readonly float transitionTime = .5f;
    private readonly float threshhold = .05f;
    private float volume = 0f;
    private AudioClip nextClip = null;
    private bool fadingOut = false;
    private bool fadingIn = false;

    private void Update()
    {
        if (fadingOut)
        {
            audioSource.volume = Mathf.SmoothDamp(audioSource.volume, 0f, ref volume , transitionTime);
            if (audioSource.volume <= threshhold) {
                audioSource.volume = 0;
                fadingOut = false;
                fadingIn = true;
                audioSource.clip = nextClip;
                audioSource.Play();
                currentClip = nextClip;
                nextClip = null;
            }
        }
        else if (fadingIn)
        {
            audioSource.volume = Mathf.SmoothDamp(audioSource.volume, 1f, ref volume, transitionTime);
            if (audioSource.volume + threshhold >= 1f)
            {
                fadingIn = false;
            }
        }
    }

    public void ChangeMusic(AudioClip newClip)
    {
        if (newClip != currentClip)
        {
            nextClip = newClip;
            fadingOut = true;
        }
    }
}
