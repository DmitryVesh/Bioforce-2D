using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameChat : UIEntryManager
{
    public static InGameChat Instance { get; private set; }

    public void AddInGameChatEntry(string text)
    {
        AddEntry(text);
    }

    protected override void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"InGameChat instance already exists, destorying {gameObject.name}");
            Destroy(this);
        }
        base.Awake();
    }
    private void Start()
    {
        GameManager.Instance.OnPlayerConnected += PlayerConnectedMessage;
        GameManager.Instance.OnPlayerDisconnected += PlayerDisconnectedMessage;
    }

    private void PlayerConnectedMessage(int iD, string username, bool justJoined)
    {
        string message;
        if (justJoined)
            message = "joined";
        else
            message = "is in game";
        AddInGameChatEntry($"Player {iD} \"{username}\" {message}");
    }
    private void PlayerDisconnectedMessage(int iD, string username)
    {
        AddInGameChatEntry($"Player {iD} \"{username}\" left");
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerConnected -= PlayerConnectedMessage;
        GameManager.Instance.OnPlayerDisconnected -= PlayerDisconnectedMessage;
    }

}
