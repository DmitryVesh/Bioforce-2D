﻿using UnityEngine;

[System.Serializable]
public class AudioTrack
{
    //All properties entered in editor
    public AudioClip AudioClip;
    public float Volume;

    public bool Loop;
    public bool FadeIn;
    public bool FadeOut;
    public float FadeInTime;
    public float FadeOutTime;
}
