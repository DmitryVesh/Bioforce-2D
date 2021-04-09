using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    [SerializeField] private GameObject PlayerPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"NetworkManager instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }
    }

    internal PlayerServer InstantiatePlayer() =>
        Instantiate(PlayerPrefab).GetComponent<PlayerServer>();
}
