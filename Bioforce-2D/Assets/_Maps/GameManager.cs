using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static Dictionary<int, PlayerManager> PlayerDictionary;
    [SerializeField] private GameObject LocalPlayerPrefab;
    [SerializeField] private GameObject PlayerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"GameManager_SampleScene instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
        PlayerDictionary = new Dictionary<int, PlayerManager>();
    }

    public void SpawnPlayer(int iD, string username, Vector3 position, Quaternion rotation)
    {
        GameObject player;
        GameObject prefab;

        if (iD == Client.instance.ClientID)
            prefab = LocalPlayerPrefab;
        else
            prefab = PlayerPrefab;

        player = Instantiate(prefab, position, rotation);
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        playerManager.Initialise(iD, username);

        PlayerDictionary.Add(iD, playerManager);
    }
}
