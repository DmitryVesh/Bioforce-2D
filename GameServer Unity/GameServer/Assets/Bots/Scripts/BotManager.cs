using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Singleton;

public class BotManager : MonoBehaviour
{
    public static BotManager Instance { get => instance; }
    private static BotManager instance;

    [SerializeField] private GameObject Bot;

    private Queue<Bot> BotsAvailable { get; set; } = new Queue<Bot>();
    public Dictionary<byte, Bot> BotDictionary { get; private set; } = new Dictionary<byte, Bot>();

    private void Awake()
    {
        Singleton.Init(ref instance, this);
    }
    
    void Start()
    {
        ResetBots();

        GameStateManager.Instance.OnServerGameActivated += SpawnBots;
        GameStateManager.Instance.OnServerGameEnded += DespawnBots;        ;
        GameStateManager.Instance.OnServerRestart += ResetBots;
    }
}
