using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private string VolParam;
    [SerializeField] private AudioMixerGroup AudioMixer;
    [SerializeField] private Slider Slider;

    private void Awake()
    {
        Slider.onValueChanged.AddListener(OnSliderChanged);

        if (AudioMixer.audioMixer.GetFloat(VolParam, out float storedValue))
        {
            storedValue = Mathf.Pow(10, storedValue / 30f);
            Slider.value = PlayerPrefs.GetFloat(VolParam, storedValue);
        }
        else
            Slider.value = 1;
    }

    private void OnSliderChanged(float sliderValue)
    {
        AudioMixer.audioMixer.SetFloat(VolParam, Mathf.Log10(sliderValue) * 30f);
        PlayerPrefs.SetFloat(VolParam, sliderValue);
    }
}
