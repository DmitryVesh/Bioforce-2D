using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;
using GameServer;
using System.Linq;

public class BotManager : MonoBehaviour
{
    public static BotManager Instance { get => instance; }
    private static BotManager instance;

    [SerializeField] private GameObject Bot;
    /// <summary>
    /// Stores bots that are available to be added into game, if a player quits. Acting as a bot cache
    /// </summary>
    private Queue<Bot> BotsAvailable { get; set; } = new Queue<Bot>();
    /// <summary>
    /// Stores actively used bots that are in game now
    /// </summary>
    public Dictionary<ushort, Bot> BotDictionary { get; private set; } = new Dictionary<ushort, Bot>();
    private ushort NumBotsExistedCount = 0;



    private void Awake()
    {
        Singleton.Init(ref instance, this);
    }
    
    void Start()
    {
        ResetAllBots();

        GameStateManager.Instance.OnPlayerJoinedGame += DespawnBotWithLeastKills;
        GameStateManager.Instance.OnPlayerLeftGame -= SpawnBot;

        GameStateManager.Instance.OnServerGameActivated += SpawnAllBots;
        GameStateManager.Instance.OnServerGameEnded += DespawnAllBots;
        GameStateManager.Instance.OnServerRestart += ResetAllBots;
    }
    private void OnDestroy()
    {        
        GameStateManager.Instance.OnPlayerJoinedGame -= DespawnBotWithLeastKills;
        GameStateManager.Instance.OnPlayerLeftGame -= SpawnBot;

        GameStateManager.Instance.OnServerGameActivated -= SpawnAllBots;
        GameStateManager.Instance.OnServerGameEnded -= DespawnAllBots;
        GameStateManager.Instance.OnServerRestart -= ResetAllBots;
    }

    

    private void DespawnBot(Bot bot, ushort botID)
    {
        if (BotDictionary.Count <= 0) //Wait for SpawnBots if there are no active bots
            return;

        bot.Despawn();
        bot.SetActive(false);

        BotsAvailable.Enqueue(bot);
        BotDictionary.Remove(botID);

        ServerSend.DespawnBot(bot);
    }
    private void DespawnAllBots()
    {
        foreach (var bot in BotDictionary)
            DespawnBot(bot.Value, bot.Key);
    }
    private void DespawnBotWithLeastKills()
    {
        var botID = GetBotWithLeastKills();
        var bot = BotDictionary[botID];

        DespawnBot(bot, botID);
    }
    private ushort GetBotWithLeastKills()
    {
        var botMinKillsID = (ushort.MaxValue);
        var botMinKills = int.MaxValue;

        foreach (var bot in BotDictionary)
        {
            var kills = bot.Value.Player.Kills;
            if (kills > botMinKills)
            {
                botMinKills = kills;
                botMinKillsID = bot.Key;
            }
        }

        return botMinKillsID;
    }

    private void SpawnBot()
    {
        var bot = BotsAvailable.Dequeue();

        bot.SetActive(true);
        bot.Spawn(RespawnPoint.GetRandomSpawnPoint());

        bot.BotID = NumBotsExistedCount;
        BotDictionary.Add(NumBotsExistedCount++, bot);

        ServerSend.SpawnBot(bot);
    }
    private void SpawnAllBots(float _)
    {
        for (int i = 0; i < Server.MaxNumPlayers; i++)
            SpawnBot();
    }

    private void ResetAllBots()
    {
        BotsAvailable.Clear();
        BotDictionary.Clear();

        for (int i = 0; i < Server.MaxNumPlayers; i++)
            BotsAvailable.Enqueue(new Bot(GetRandomBotType()));
    }
    private BotType GetRandomBotType()
    {
        var botTypes =  (BotType[])Enum.GetValues(typeof(BotType));
        var randomBotTypeIndex = UnityEngine.Random.Range(0, botTypes.Length);
        return botTypes[randomBotTypeIndex];
    }
}
