using GameServer;
using System;
using System.Collections;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.Singleton;

public enum GameState
{
    waitingForAPlayerToJoin,
    activeGameInProcess,
    gameEnded,
    gameRestarting,
    serverShuttingDown
}
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get => instance; }
    private static GameStateManager instance;

    public GameState CurrentState { get; private set; } = GameState.waitingForAPlayerToJoin;
    private void UpdateCurrentGameState(GameState state)
    {
        CurrentState = state;
        ServerSend.SendGameState(CurrentState, RemainingGameTime);
    }

    public Action<float> OnServerGameActivated { get; set; }
    public Action OnServerGameEnded { get; set; }

    //Need to implement
    public Action OnServerRestart { get; set; } //Each ClientServer will subscribe to this so the player model is destroyed, and is taken back to AskPlayerDetails 
    public Action OnServerStop { get; set; }
    public Action OnServerShutdown { get; set; }
    //

    [SerializeField] private float GameTimeSeconds = 180f; //Game should last for 3min = 60sec/min * 3min = 180s
    private float StartGameTime { get; set; }
    private float FinGameTime { get; set; }
    public float RemainingGameTime
    {
        get
        {
            if (!CurrentState.Equals(GameState.activeGameInProcess))
                return -1f;

            return FinGameTime - Time.fixedTime;
        }
    }

    [SerializeField] private float TimeForGameEndSeconds = 15f;
    [SerializeField] private float TimeForServerShutdownSeconds = 10f;

    internal void PlayerJoinedServer()
    {
        if (!CurrentState.Equals(GameState.waitingForAPlayerToJoin) &&
            !CurrentState.Equals(GameState.gameRestarting)) //Prevents more than 1 call
            return;

        
        StartGameTime = Time.fixedTime;
        FinGameTime = StartGameTime + GameTimeSeconds;

        OnServerGameActivated?.Invoke(RemainingGameTime);

        UpdateCurrentGameState(GameState.activeGameInProcess);

        StartCoroutine(EndGameIn(RemainingGameTime));
    }

    private IEnumerator EndGameIn(float remainingGameTime)
    {
        yield return new WaitForSeconds(remainingGameTime);
        UpdateCurrentGameState(GameState.gameEnded);

        OnServerGameEnded?.Invoke();

        if (MainServerComms.ServerEndsWhenNoPlayersOn)
            StartCoroutine(CloseTheServer());
        else
            StartCoroutine(RestartServer());
    }

    private IEnumerator RestartServer()
    {
        yield return new WaitForSeconds(TimeForGameEndSeconds);
        UpdateCurrentGameState(GameState.gameRestarting);
        OnServerRestart?.Invoke();
    }

    private IEnumerator CloseTheServer()
    {
        yield return new WaitForSeconds(TimeForGameEndSeconds);
        //Server send to all players that the server is closing
        //Send to MainServer that the server is shutting down
        //Not accept any other connection attempts by other players
        UpdateCurrentGameState(GameState.serverShuttingDown);
        OnServerStop?.Invoke();
        yield return new WaitForSeconds(TimeForServerShutdownSeconds);
        OnServerShutdown?.Invoke();
    }

    private void Awake()
    {
        Singleton.Init(ref instance, this);
    }
}
