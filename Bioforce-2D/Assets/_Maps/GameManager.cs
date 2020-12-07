using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }
    public static Dictionary<int, PlayerManager> PlayerDictionary { get; set; }

    [SerializeField] private bool TestingTouchInEditor = false;
    [SerializeField] private GameObject LocalPlayerPrefab;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject MobileLocalPlayerPrefab;

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

    public void SpawnPlayer(int iD, string username, Vector3 position, Quaternion rotation, bool isDead)
    {
        GameObject player;
        GameObject prefab;

        if (iD == Client.Instance.ClientID)
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
        player = Instantiate(prefab, position, rotation);

        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        playerManager.Initialise(iD, username);
        PlayerDictionary.Add(iD, playerManager);

        NonLocalPlayerHealth healthManager = player.GetComponentInChildren<NonLocalPlayerHealth>();
        healthManager?.SetOwnerClientID(iD);

        NonLocalPlayerMovement playerMovement = player.GetComponentInChildren<NonLocalPlayerMovement>();
        playerMovement?.SetOwnerClientID(iD);
        
        NonLocalPlayerAnimations playerAnimations = player.GetComponentInChildren<NonLocalPlayerAnimations>();
        playerAnimations?.SetOwnerClientID(iD);

        StartCoroutine(playerManager.IsPlayerDeadUponSpawning(isDead));
    }
    public void DisconnectPlayer(int disconnectedPlayer)
    {
        Debug.Log($"Player: {disconnectedPlayer} has disconnected.");
        PlayerDictionary[disconnectedPlayer].Disconnect();
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
    public void PlayerDied(int playerKilledDiedID, int bulletOwnerID)
    {
        ClientSend.PlayerDied(bulletOwnerID);
        PlayerDictionary[playerKilledDiedID].PlayerDied();
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
