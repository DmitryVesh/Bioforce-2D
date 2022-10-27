using GameServer;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Singleton;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get => instance; }
    private static NetworkManager instance;

    [SerializeField] private GameObject PlayerPrefab;

    private bool HasDataToSend { get; set; } = false;
    private List<(byte, ConstantlySentPlayerData)> PlayerDatas { get; set; } = new List<(byte, ConstantlySentPlayerData)>();

    private void Awake()
    {
        Singleton.Init(ref instance, this);
    }

    internal PlayerServer InstantiatePlayer() =>
        Instantiate(PlayerPrefab).GetComponent<PlayerServer>();
    internal void InstantiateBots(int maxNumPlayers)
    {
        for (int i = 0; i < maxNumPlayers; i++)
            InstatiateBot();
    }
    internal void InstatiateBot()
    {

    }

    internal void AddPlayerDataToBeSynchronised(byte iD, ConstantlySentPlayerData playerData)
    {
        HasDataToSend = true;
        PlayerDatas.Add((iD, playerData.Clone()));
    }

    private void FixedUpdate()
    {
        if (HasDataToSend)
        {
            ServerSend.ConstantPlayerData(PlayerDatas);
            HasDataToSend = false;
            PlayerDatas.Clear();
        }
    }

    
}
