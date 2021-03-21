using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMusicManager : MonoBehaviour
{
    public static SoundMusicManager Instance { get; set; }
    private Transform AudioListenerTF { get; set; }

    private AudioSource MusicAudioSource { get; set; }

    [SerializeField] private AudioTrack[] MainMenuMusic = null;
    [SerializeField] private AudioTrack[] Level1Music = null;
    private string MainMenu = "Main Menu";
    private string Level1 = "Level 1";

    private Dictionary<string, AudioTrack[]> ScenesMusic = new Dictionary<string, AudioTrack[]>();
    
    private Action OnMusicEndAction { get; set; }

    private int[] CurrentSceneTracksOrder { get; set; }
    private int CurrentSceneTracksOrderIndex { get; set; }
    private string CurrentScene { get; set; }
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
        
        InitiliseScenesMusic();
    }

    private void InitiliseScenesMusic()
    {
        AddSceneMusic(MainMenu, MainMenuMusic);
        AddSceneMusic(Level1, Level1Music);
    }
    private void AddSceneMusic(string sceneName, AudioTrack[] music) 
    {
        ScenesMusic.Add(sceneName, music);
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

        GameManager.Instance.OnLoadSceneEvent += PlaySceneMusic;
        OnMusicEndAction += PlayCurrentSceneMusic;

        //Start in Main Menu so play Main Menu Music
        PlaySceneMusic(MainMenu);
    }

    private void PlaySceneMusic(string sceneName)
    {
        CurrentScene = sceneName;
        AudioTrack[] music = ScenesMusic[CurrentScene];

        int numTracks = music.Length;
        
        CurrentSceneTracksOrder = new int[numTracks];
        for (int i = 0; i < numTracks; i++)
            CurrentSceneTracksOrder[i] = -1;

        CurrentSceneTracksOrderIndex = 0;
        for (int trackOrderCount = 0; trackOrderCount < numTracks; trackOrderCount++)
        {
            int trackNum = UnityEngine.Random.Range(0, numTracks);
            if (CurrentSceneTracksOrder.Contains(trackNum))
            {
                trackOrderCount--;
                continue;
            }

            CurrentSceneTracksOrder[trackOrderCount] = trackNum;
        }

        PlayCurrentSceneMusic();
    }

    private void PlayCurrentSceneMusic()
    {
        ChangeMusic(ScenesMusic[CurrentScene][CurrentSceneTracksOrder[CurrentSceneTracksOrderIndex]]);
        CurrentSceneTracksOrderIndex = (CurrentSceneTracksOrderIndex + 1) % ScenesMusic[CurrentScene].Length;
    }

    private void FixedUpdate()
    {
        if (AudioListenerTF == null)
            AudioListenerTF = Camera.main.transform;
        transform.position = AudioListenerTF.position;
    }

    private void ChangeMusic(AudioTrack audioTrack) 
    {
        StartCoroutine(ActuallyChangeMusic(audioTrack));
    }
    
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
        float fadeSpeed = 1 / CurrentAudioTrack.FadeInTime * Time.fixedDeltaTime;

        while(MusicAudioSource.volume < maxVolume && KeepFadingIn)
        {
            MusicAudioSource.volume += fadeSpeed;
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
        }
        KeepFadingIn = false;
    }
    private IEnumerator FadeOut()
    {
        if (CurrentAudioTrack is null || !CurrentAudioTrack.FadeOut)
        {
            KeepFadingOut = false;
            yield break;
        }

        KeepFadingIn = false;
        KeepFadingOut = true;
        
        float fadeSpeed = 1 / CurrentAudioTrack.FadeOutTime * Time.fixedDeltaTime;

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

        StartCoroutine(CallOnMusicEndFinishSuccessful(audioTrack));
        if (audioTrack.FadeIn && !dontFadeOverride)
            StartCoroutine(FadeIn());
        if (audioTrack.Loop)
            StartCoroutine(Loop(audioTrack));
        
    }
    private IEnumerator CallOnMusicEndFinishSuccessful(AudioTrack track)
    {
        yield return new WaitForSecondsRealtime(track.AudioClip.length);
        if (MusicAudioSource.clip == track.AudioClip)
        {
            OnMusicEndAction?.Invoke();
        }
    }
}
