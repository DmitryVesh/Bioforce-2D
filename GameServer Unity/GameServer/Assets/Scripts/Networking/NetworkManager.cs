using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NetworkManagerState
{
    waitingForAPlayerToJoin,
    activeGameInProcess
}
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    
    [SerializeField] private GameObject PlayerPrefab;


    public NetworkManagerState CurrentState { get; private set; } = NetworkManagerState.waitingForAPlayerToJoin;
    public Action<float> OnServerActivated { get; set; }

    [SerializeField] private float GameTime = 240f; //Game should last for 4m = 240s
    private float StartGameTime { get; set; }
    private float FinGameTime { get; set; }
    public float RemainingGameTime
    {
        get
        {
            if (!CurrentState.Equals(NetworkManagerState.activeGameInProcess))
                return -1f;

            return FinGameTime - Time.fixedTime;
        }
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Output.WriteLine($"NetworkManager instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }
    }

    internal void PlayerJoinedServer()
    {
        if (!CurrentState.Equals(NetworkManagerState.waitingForAPlayerToJoin))
            return;

        CurrentState = NetworkManagerState.activeGameInProcess;
        StartGameTime = Time.fixedTime;
        FinGameTime = StartGameTime + GameTime;

        OnServerActivated?.Invoke(RemainingGameTime);
    }
    internal PlayerServer InstantiatePlayer() =>
        Instantiate(PlayerPrefab).GetComponent<PlayerServer>();
}
