using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMusicManager : MonoBehaviour
{
    public static SoundMusicManager Instance { get; set; }
    private Transform AudioListenerTF { get; set; }

    private AudioSource MusicAudioSource { get; set; }
    [SerializeField] private AudioTrack StartingMusic = null;

    private AudioTrack CurrentAudioTrack { get; set; }
    private bool KeepFadingIn { get; set; }
    private bool KeepFadingOut { get; set; }
    private bool KeepLooping { get; set; }

    [SerializeField] private AudioClip[] ButtonPressedSFXs;
    [SerializeField] private AudioClip[] ButtonSelectedSFXs;
    [SerializeField] private AudioClip[] ButtonDeselectedSFXs;

    [SerializeField] private AudioMixerGroup MasterMixer;
    [SerializeField] private AudioMixerGroup MusicMixer;
    [SerializeField] private AudioMixerGroup SFXMixer;

    [SerializeField] private AudioSource SoundEffectAudioSource;

    public static AudioClip GetRandomAudioClip(AudioClip[] audioClips) =>
        audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
    
    public static void PlayMainMenuSFX(MainMenuSFXs mainMenuSFX)
    {
        AudioClip SFX = null;
        switch (mainMenuSFX)
        {
            case MainMenuSFXs.buttonPressed:
                SFX = GetRandomAudioClip(Instance.ButtonPressedSFXs);
                break;
            case MainMenuSFXs.buttonSelected:
                SFX = GetRandomAudioClip(Instance.ButtonSelectedSFXs);
                break;
            case MainMenuSFXs.buttonDeselected:
                SFX = GetRandomAudioClip(Instance.ButtonDeselectedSFXs);
                break;
        }
        Instance.SoundEffectAudioSource.PlayOneShot(SFX);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"SoundMusicManager instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }

        MusicAudioSource = GetComponent<AudioSource>();
        AudioListenerTF = Camera.main.transform;

    } 
    
    private void SetMixerStored(string param, ref AudioMixerGroup mixer, float defaultVal)
    {
        float val = PlayerPrefs.GetFloat(param, defaultVal);
        mixer.audioMixer.SetFloat(param, Mathf.Log10(val) * 30f);
    }

    private void Start()
    {
        SetMixerStored("Master", ref MasterMixer, 0.8f);
        SetMixerStored("Music", ref MusicMixer, 1);
        SetMixerStored("Sound Effects", ref SFXMixer, 1);
        PlayClip(StartingMusic);
    }
    private void FixedUpdate()
    {
        if (AudioListenerTF == null)
            AudioListenerTF = Camera.main.transform;
        transform.position = AudioListenerTF.position;
    }

    private void ChangeMusic(AudioTrack audioTrack) =>
        StartCoroutine(ActuallyChangeMusic(audioTrack));
    
    private IEnumerator ActuallyChangeMusic(AudioTrack audioTrack)
    {
        StartCoroutine(FadeOut());
        while (KeepFadingOut)
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);

        MusicAudioSource.Stop();
        PlayClip(audioTrack);
    }
    private IEnumerator FadeIn()
    {
        KeepFadingIn = true;
        KeepFadingOut = false;

        MusicAudioSource.volume = 0;
        float maxVolume = CurrentAudioTrack.Volume;
        float fadeSpeed = 1 / CurrentAudioTrack.FadeTime * Time.fixedDeltaTime;

        while(MusicAudioSource.volume < maxVolume && KeepFadingIn)
        {
            MusicAudioSource.volume += fadeSpeed;
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
        }
        KeepFadingIn = false;
    }
    private IEnumerator FadeOut()
    {
        KeepFadingIn = false;
        KeepFadingOut = true;

        float fadeSpeed = 1 / CurrentAudioTrack.FadeTime * Time.fixedDeltaTime;

        while (MusicAudioSource.volume > 0 && KeepFadingOut)
        {
            MusicAudioSource.volume -= fadeSpeed;
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
        MusicAudioSource.clip = audioTrack.AudioClip;
        MusicAudioSource.volume = audioTrack.Volume;
        MusicAudioSource.Play();
        CurrentAudioTrack = audioTrack;
        if (audioTrack.Fade && !dontFadeOverride)
            StartCoroutine(FadeIn());
        if (audioTrack.Loop)
            StartCoroutine(Loop(audioTrack));
        
    }
}
