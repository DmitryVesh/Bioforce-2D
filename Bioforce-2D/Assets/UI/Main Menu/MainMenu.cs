﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MainMenuSFXs
{
    buttonPressed,
    buttonSelected,
    buttonDeselected
}

[RequireComponent(typeof(AudioSource))]
public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; set; }

    [SerializeField] private List<MenuButton> MenuButtons;
    private int CurrentSelectedButtonIndex { get; set; }
    private int LastSelectedButtonIndex { get; set; }

    private bool KeyIsDown { get; set; }

    private AudioSource AudioSource { get; set; }
    private GameObject MainMenuPanel { get; set; }

    [SerializeField] private AudioClip[] ButtonPressedSFXs;
    [SerializeField] private AudioClip[] ButtonSelectedSFXs;
    [SerializeField] private AudioClip[] ButtonDeselectedSFXs;

    //Specific Button Click events - the MenuButton subscribes to these events via the Inspector
    public void MultiplayerButtonClicked()
    {
        ServerMenu.Instance.ShowServerMenu();
        HideMainMenu();
    }
    public void SingleplayerButtonClicked()
    {

    }
    public void SettingsButtonClicked()
    {

    }
    public void TutorialButtonClicked()
    {

    }
    public void QuitButtonClicked()
    {
        Application.Quit();
    }

    public void BackToMainMenu() =>
        MainMenuPanel.SetActive(true);

    private void HideMainMenu() =>
        MainMenuPanel.SetActive(false);

    public void SetButtonSelected(MenuButton menuButton)
    {
        CurrentSelectedButtonIndex = MenuButtons.IndexOf(menuButton);
        if (CurrentSelectedButtonIndex == LastSelectedButtonIndex)
            return;

        MenuButtons[LastSelectedButtonIndex].OnPointerExit(null);
        LastSelectedButtonIndex = CurrentSelectedButtonIndex;
    }
    public void PlayMainMenuSFX(MainMenuSFXs mainMenuSFX)
    {
        AudioClip SFX = null;
        switch (mainMenuSFX)
        {
            case MainMenuSFXs.buttonPressed:
                SFX = SoundMusicManager.GetRandomAudioClip(ButtonPressedSFXs);
                break;
            case MainMenuSFXs.buttonSelected:
                SFX = SoundMusicManager.GetRandomAudioClip(ButtonSelectedSFXs);
                break;
            case MainMenuSFXs.buttonDeselected:
                SFX = SoundMusicManager.GetRandomAudioClip(ButtonDeselectedSFXs);
                break;
        }
        AudioSource.PlayOneShot(SFX);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"MainMenu instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
        AudioSource = GetComponent<AudioSource>();
        MainMenuPanel = transform.GetChild(0).gameObject;
    }
    private void Start()
    {
        //Set default selected button
        MenuButtons[CurrentSelectedButtonIndex].OnPointerEnter(null);
    }

    private void Update()
    {
        if (!MainMenuPanel.activeInHierarchy)
            return;

        if (Input.GetButtonDown("Enter"))
            MenuButtons[CurrentSelectedButtonIndex].OnPointerClick(null);

        if (Input.GetAxis("Vertical") == 0)
        {
            KeyIsDown = false;
            return;
        }
        if (KeyIsDown)
            return;
        

        //MenuButtons[LastSelectedButtonIndex].OnPointerExit(null);
        if (Input.GetAxis("Vertical") < 0) //Pressing down or s key
        {
            if (CurrentSelectedButtonIndex < MenuButtons.Count - 1)
                CurrentSelectedButtonIndex++;
            else
                CurrentSelectedButtonIndex = 0;
        }
        else //Pressing up or w key
        {
            if (CurrentSelectedButtonIndex > 0)
                CurrentSelectedButtonIndex--;
            else
                CurrentSelectedButtonIndex = MenuButtons.Count - 1;
        }
        MenuButtons[CurrentSelectedButtonIndex].OnPointerEnter(null);
        KeyIsDown = true;
        LastSelectedButtonIndex = CurrentSelectedButtonIndex;
    }
}