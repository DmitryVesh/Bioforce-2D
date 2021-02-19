using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonManager : MonoBehaviour
{
    public static MenuButtonManager Instance { get; set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"MenuButtonSounds instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }
    }

}
