using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public static Minimap Instance { get; private set; }
    private List<MinimapIcon> Icons { get; set; } = new List<MinimapIcon>();


    internal static void SubscribeIcon(MinimapIcon minimapIcon) =>
        Instance.Icons.Add(minimapIcon);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"GameManager instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
