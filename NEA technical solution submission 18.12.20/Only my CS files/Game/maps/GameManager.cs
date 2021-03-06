﻿using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }
    public static Dictionary<int, PlayerManager> PlayerDictionary { get; set; }

    [SerializeField] private bool TestingTouchInEditor = false;
    [SerializeField] private GameObject LocalPlayerPrefab;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject MobileLocalPlayerPrefab;

    public delegate void PlayerConnected (int iD, string username, bool justJoined);
    public event PlayerConnected OnPlayerConnected;

    public delegate void PlayerDisconnected (int iD, string username);
    public event PlayerDisconnected OnPlayerDisconnected;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"GameManager instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
        PlayerDictionary = new Dictionary<int, PlayerManager>();
    }

    public void SpawnPlayer(int iD, string username, Vector3 position, bool isFacingRight, bool isDead, bool justJoined)
    {
        GameObject player;
        GameObject prefab;

        bool localClient = iD == Client.Instance.ClientID;

        if (localClient)
        {
            if (IsMobileSupported())
                prefab = MobileLocalPlayerPrefab;
            else 
                prefab = LocalPlayerPrefab;
            Debug.Log($"You, player: {iD} have been spawned.");
        }
        else
        {
            prefab = PlayerPrefab;
            Debug.Log($"Player: {iD} has been spawned.");
        }
        player = Instantiate(prefab, position, Quaternion.identity);

        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        playerManager.Initialise(iD, username);
        PlayerDictionary.Add(iD, playerManager);

        NonLocalPlayerHealth healthManager = player.GetComponentInChildren<NonLocalPlayerHealth>();
        healthManager?.SetOwnerClientID(iD);

        NonLocalPlayerMovement playerMovement = player.GetComponentInChildren<NonLocalPlayerMovement>();
        playerMovement?.SetOwnerClientID(iD);
        
        NonLocalPlayerAnimations playerAnimations = player.GetComponentInChildren<NonLocalPlayerAnimations>();
        playerAnimations?.SetOwnerClientID(iD);

        IGun playerGun = player.GetComponentInChildren<IGun>();
        playerGun?.SetOwnerClientID(iD);

        if (!isFacingRight)
            playerAnimations.FlipSprite();

        StartCoroutine(playerManager.IsPlayerDeadUponSpawning(isDead));

        OnPlayerConnected?.Invoke(iD, username, justJoined);
    }
    public void DisconnectPlayer(int disconnectedPlayer)
    {
        Debug.Log($"Player: {disconnectedPlayer} has disconnected.");
        PlayerManager playerManager = PlayerDictionary[disconnectedPlayer];

        OnPlayerDisconnected?.Invoke(disconnectedPlayer, playerManager.GetUsername());

        playerManager.Disconnect();
        PlayerDictionary.Remove(disconnectedPlayer);        
    }
    public void DisconnectAllPlayers()
    {
        Debug.Log($"All players are being disconnected.");
        foreach (PlayerManager player in PlayerDictionary.Values)
        {
            player.Disconnect();
        }
        PlayerDictionary.Clear();
    }
    public void PlayerDied(int playerKilledDiedID, int bulletOwnerID, TypeOfDeath typeOfDeath)
    {
        ClientSend.PlayerDied(bulletOwnerID, typeOfDeath);
        PlayerDictionary[playerKilledDiedID].PlayerDied(typeOfDeath);
        KillFeedUI.Instance.AddKillFeedEntry(playerKilledDiedID, bulletOwnerID);
    }
    public void PlayerRespawned(int iD)
    {
        ClientSend.PlayerRespawned();
        PlayerDictionary[iD].PlayerRespawned();
    }
    private bool IsMobileSupported()
    {
        bool result;
        RuntimePlatform platform = Application.platform;

        if (TestingTouchInEditor && (platform.Equals(RuntimePlatform.WindowsEditor) || platform.Equals(RuntimePlatform.OSXEditor)))
            result = true;
        else if (platform.Equals(RuntimePlatform.IPhonePlayer) || platform.Equals(RuntimePlatform.Android) || platform.Equals(RuntimePlatform.Lumin))
            result = true;
        else
            result = false;
        return result;
    }
}
