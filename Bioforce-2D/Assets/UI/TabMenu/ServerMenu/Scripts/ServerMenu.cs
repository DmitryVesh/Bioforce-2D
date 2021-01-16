using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerMenu : MonoBehaviour
{
    public static ServerMenu Instance;

    private GameObject ServerMenuHolder { get; set; }

    public void ShowServerMenu() =>
        ServerMenuHolder.SetActive(false);
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
        ServerMenuHolder = transform.GetChild(0).gameObject;
        ServerMenuHolder.SetActive(false);
    }

    
}
