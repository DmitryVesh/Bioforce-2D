using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerMenu : MonoBehaviour
{
    public static ServerMenu Instance;

    private GameObject ServerMenuPanel { get; set; }
    private GameObject PlayerRegistrationPanel { get; set; }

    [SerializeField] private MenuButton ConnectButton;

    public void ShowServerMenu()
    {
        if (!PlayerPrefs.HasKey("Username"))
            DisplayUserRegistration();
        else
            ServerMenuPanel.SetActive(true);
    }
    public void HideServerMenu() =>
        ServerMenuPanel.SetActive(false);
    public void DisplayUserRegistration()
    {
        HideServerMenu();
        PlayerRegistrationPanel.SetActive(true);
        PlayerRegistration.Instance.DisplayUserRegistration();
    }

    public void SetSelectedPage(GameObject selectedPage)
    {
        
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"ServerMenu instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
        ServerMenuPanel = transform.GetChild(0).gameObject;
        PlayerRegistrationPanel = transform.GetChild(1).gameObject;

        HideServerMenu();
        PlayerRegistrationPanel.SetActive(false);
    }
    private void Start()
    {
        ConnectButton.Interactable = false;
    }
    
}
