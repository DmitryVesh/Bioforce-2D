using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }
    public static Dictionary<byte, PlayerManager> PlayerDictionary { get; set; }

    [SerializeField] private bool TestingTouchInEditor = false;
    [SerializeField] private GameObject LocalPlayerPrefab;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject MobileLocalPlayerPrefab;

    public delegate void PlayerConnected (byte iD, string username, bool justJoined);
    public event PlayerConnected OnPlayerConnected;

    public delegate void PlayerDisconnected (byte iD, string username);

    
    public event PlayerDisconnected OnPlayerDisconnected;

    public bool IsMobileSupported { get; private set; }
    public bool InGame { get; set; }

    public delegate void OnPause(bool pause);
    public event OnPause OnPauseEvent;
    public event OnPause OnLostConnectionEvent;
    public bool Paused { get; private set; } = false;

    public delegate void LoadScene(string sceneName);
    public event LoadScene OnLoadSceneEvent;

    //public Action<Color, int> OnPlayerChosenColor;

    internal void PlayerChoseColor(Color chosenColor, int chosenColorIndex)
    {
        ClientSend.PlayerReadyToJoin(chosenColorIndex);
    }


    public static void ConfyMouse() =>
        Cursor.lockState = CursorLockMode.Confined;
    public static void ShowMouse(bool showing) =>
        Cursor.visible = showing;

    public void InvokePauseEvent(bool pause)
    {
        Paused = pause;
        OnPauseEvent?.Invoke(pause);
    }
    public void InvokeLostConnectionEvent(bool lostConnection)
    {
        OnLostConnectionEvent?.Invoke(lostConnection);
    }

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
        PlayerDictionary = new Dictionary<byte, PlayerManager>();
        ConfyMouse();
        IsMobileSupported = CheckIfOnMobile();

    }
    private void Update()
    {
        if (InGame && PressedPauseButton())
        {
            InvokePauseEvent(!Paused);
        }
    }
    
    private bool PressedPauseButton()
    {
        if (!IsMobileSupported && Input.GetButtonDown("Pause"))
            return true;
        return false;
    }

    public void SpawnPlayer(byte iD, string username, Vector3 position, bool isFacingRight, bool isDead, bool justJoined, int maxHealth, int currentHealth, int playerColorIndex)
    {
        GameObject player;
        GameObject prefab;

        bool localClient = iD == Client.Instance.ClientID;
        Color playerColor = PlayerChooseColor.Instance.GetColorFromIndex(playerColorIndex);

        if (localClient)
        {
            if (IsMobileSupported)
                prefab = MobileLocalPlayerPrefab;
            else 
                prefab = LocalPlayerPrefab;
            MobileJoystick.Instance.SetPlayerColor(playerColor);
            ShootingJoystick.Instance.SetPlayerColor(playerColor);
            Debug.Log($"You, player: {iD} have been spawned.");
        }
        else
        {
            prefab = PlayerPrefab;
            Debug.Log($"Player: {iD} has been spawned.");
        }
        player = Instantiate(prefab, position, Quaternion.identity);

        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        playerManager.Initialise(iD, username, playerColor, maxHealth);
        PlayerDictionary.Add(iD, playerManager);

        NonLocalPlayerHealth healthManager = player.GetComponentInChildren<NonLocalPlayerHealth>();
        healthManager?.SetOwnerClientID(iD);
        healthManager?.SetSpawnedHealth(maxHealth, currentHealth);

        NonLocalPlayerMovement playerMovement = player.GetComponentInChildren<NonLocalPlayerMovement>();
        playerMovement?.SetOwnerClientID(iD);
        
        NonLocalPlayerAnimations playerAnimations = player.GetComponentInChildren<NonLocalPlayerAnimations>();
        playerAnimations?.SetOwnerClientID(iD);
        playerAnimations?.SetColor(playerColor);

        IGun playerGun = player.GetComponentInChildren<IGun>();
        playerGun?.SetOwnerClientID(iD);
        playerGun?.SetColor(playerColor);
        playerGun?.SetOwnerCollider(player.GetComponentInChildren<CapsuleCollider2D>());

        if (!isFacingRight)
            playerAnimations.FlipSprite();

        StartCoroutine(playerManager.IsPlayerDeadUponSpawning(isDead));


        OnPlayerConnected?.Invoke(iD, username, justJoined);
    }
    public void DisconnectPlayer(byte disconnectedPlayer)
    {
        try 
        {
            PlayerManager playerManager = PlayerDictionary[disconnectedPlayer];

            OnPlayerDisconnected?.Invoke(disconnectedPlayer, playerManager.GetUsername());

            playerManager.Disconnect();
            PlayerDictionary.Remove(disconnectedPlayer);
            Debug.Log($"Player: {disconnectedPlayer} has disconnected.");

        }
        catch (KeyNotFoundException exception)
        {
            Debug.Log($"Error in Disconnecting Player: {disconnectedPlayer}...\n{exception}");
        }
    }
    public void DisconnectLoadMainMenu()
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            Debug.Log($"All players are being disconnected.");
            foreach (PlayerManager player in PlayerDictionary.Values)
            {
                ScoreboardManager.Instance.DeleteEntry(player.ID);
                player.Disconnect();
            }
            PlayerDictionary.Clear();

            SwitchScene("Main Menu");
        });
        
    }

    public void SwitchScene(string sceneName)
    {
        OnLoadSceneEvent?.Invoke(sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void PlayerDied(byte playerKilledDiedID, byte bulletOwnerID, TypeOfDeath typeOfDeath)
    {
        ClientSend.PlayerDied(bulletOwnerID, typeOfDeath);
        PlayerDictionary[playerKilledDiedID].PlayerDied(typeOfDeath);
        KillFeedUI.Instance.AddKillFeedEntry(playerKilledDiedID, bulletOwnerID);
    }
    public void PlayerRespawned(byte iD)
    {
        PlayerDictionary[iD].PlayerRespawned();
        ClientSend.PlayerRespawned(PlayerDictionary[iD].RespawnPosition);
    }
    
    
    private bool CheckIfOnMobile()
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
