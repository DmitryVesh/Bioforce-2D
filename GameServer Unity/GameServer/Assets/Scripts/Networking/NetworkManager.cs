using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Singleton;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;// { get; private set; }
    
    [SerializeField] private GameObject PlayerPrefab;

    private void Awake()
    {
        Singleton.Init(ref Instance, this);
    }

    internal PlayerServer InstantiatePlayer() =>
        Instantiate(PlayerPrefab).GetComponent<PlayerServer>();
}
