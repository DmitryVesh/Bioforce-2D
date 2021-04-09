using UnityEngine;

public class InGameChat : UIEntryManager
{
    public static InGameChat Instance { get; private set; }

    public void AddInGameChatEntry(string text) =>
        AddEntry(text, null, null);

    protected override void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"InGameChat instance already exists, destorying {gameObject.name}");
            Destroy(gameObject);
        }
        base.Awake();
    }
    private void Start()
    {
        GameManager.Instance.OnPlayerConnected += PlayerConnectedMessage;
        GameManager.Instance.OnPlayerDisconnected += PlayerDisconnectedMessage;
    }

    private void PlayerConnectedMessage(byte iD, string username, bool justJoined)
    {
        string message;
        if (justJoined)
            message = "joined";
        else
            message = "is in game";
        AddInGameChatEntry($"Player {iD} \"{username}\" {message}");
    }
    private void PlayerDisconnectedMessage(byte iD, string username) =>
        AddInGameChatEntry($"Player {iD} \"{username}\" left");

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerConnected -= PlayerConnectedMessage;
        GameManager.Instance.OnPlayerDisconnected -= PlayerDisconnectedMessage;
    }

}
