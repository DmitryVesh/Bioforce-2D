using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMusicManager : MonoBehaviour
{
    public static SoundMusicManager Instance { get; set; }
    private Transform AudioListenerTF { get; set; }

    private AudioSource AudioSource { get; set; }
    [SerializeField] private AudioTrack StartingMusic = null;

    private AudioTrack CurrentAudioTrack { get; set; }
    private bool KeepFadingIn { get; set; }
    private bool KeepFadingOut { get; set; }
    private bool KeepLooping { get; set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"MusicManager instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }

        AudioSource = GetComponent<AudioSource>();
        AudioListenerTF = Camera.main.transform;
    }
    private void Start() =>
        PlayClip(StartingMusic);
    private void FixedUpdate() =>
        transform.position = AudioListenerTF.position;

    private void ChangeMusic(AudioTrack audioTrack) =>
        StartCoroutine(ActuallyChangeMusic(audioTrack));
    
    private IEnumerator ActuallyChangeMusic(AudioTrack audioTrack)
    {
        StartCoroutine(FadeOut());
        while (KeepFadingOut)
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);

        AudioSource.Stop();
        PlayClip(audioTrack);
    }
    private IEnumerator FadeIn()
    {
        KeepFadingIn = true;
        KeepFadingOut = false;

        AudioSource.volume = 0;
        float maxVolume = CurrentAudioTrack.Volume;
        float fadeSpeed = 1 / CurrentAudioTrack.FadeTime * Time.fixedDeltaTime;

        while(AudioSource.volume < maxVolume && KeepFadingIn)
        {
            AudioSource.volume += fadeSpeed;
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
        }
        KeepFadingIn = false;
    }
    private IEnumerator FadeOut()
    {
        KeepFadingIn = false;
        KeepFadingOut = true;

        float fadeSpeed = 1 / CurrentAudioTrack.FadeTime * Time.fixedDeltaTime;

        while (AudioSource.volume > 0 && KeepFadingOut)
        {
            AudioSource.volume -= fadeSpeed;
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
        }
        KeepFadingOut = false;
    }
    private IEnumerator Loop(AudioTrack audioTrack)
    {
        KeepLooping = true;
        yield return new WaitForSecondsRealtime(audioTrack.AudioClip.length);
        if (KeepLooping)
            PlayClip(audioTrack, true);
    }

    private void PlayClip(AudioTrack audioTrack, bool dontFadeOverride = false)
    {
        AudioSource.clip = audioTrack.AudioClip;
        AudioSource.volume = audioTrack.Volume;
        AudioSource.Play();
        CurrentAudioTrack = audioTrack;
        if (audioTrack.Fade && !dontFadeOverride)
            StartCoroutine(FadeIn());
        if (audioTrack.Loop)
            StartCoroutine(Loop(audioTrack));
        
    }

    
}
