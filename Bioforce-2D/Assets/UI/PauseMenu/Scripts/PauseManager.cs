using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; set; }
    public bool Paused { get; set; }

    [SerializeField] private TextMeshProUGUI TitleText;

    [SerializeField] private Menu MenuPause;
    [SerializeField] private Menu MenuSettings;
    [SerializeField] private Menu MenuAudioSettings;

    [SerializeField] private GameObject Panel;
    private Menu CurrentMenu { get; set; }
    private Menu LastMenu { get; set; }

    private bool ClickedOdd { get; set; }
    [SerializeField] private GameObject MobileSettingsButton;

    internal static void SetActive(bool active)
    {
        Instance.Paused = active;
        Instance.Panel.SetActive(active);
        if (GameManager.Instance.IsMobileSupported)
            Instance.MobileSettingsButton.SetActive(!active);

        if (active)
        {
            Instance.DeactivateAllMenus();
            Instance.ActivateMenu(Instance.MenuPause);
        }
    }
    public void OnMobileSettings()
    {
        ClickedOdd = !ClickedOdd;
        if (ClickedOdd)
            GameManager.Instance.InvokePauseEvent(!Paused);
    }

    public void OnBackToGameButton() =>
        GameManager.Instance.InvokePauseEvent(false);
    public void OnSettingsButton() =>
        ActivateMenu(MenuSettings);
    public void OnQuitButton() =>
        Client.Instance.Disconnect();

    public void OnMainMenuButton() =>
        ActivateMenu(MenuPause);
    public void OnAudioSettingsButton() =>
        ActivateMenu(MenuAudioSettings);
    public void OnBackToLastMenu() =>
        ActivateMenu(LastMenu);


    private void DeactivateAllMenus()
    {
        MenuPause.Deactivate();
        MenuSettings.Deactivate();
        MenuAudioSettings.Deactivate();
    }

    private void ActivateMenu(Menu menu)
    {
        LastMenu = CurrentMenu;
        if (CurrentMenu != null)
            CurrentMenu.Deactivate();
        TitleText.text = menu.Activate();
        CurrentMenu = menu;
    }

    

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"PauseMenu instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }

        SetActive(false);
        MobileSettingsButton.SetActive(GameManager.Instance.IsMobileSupported);

    }
    private void Start()
    {
        GameManager.Instance.OnPauseEvent += SetActive;
    }

}
