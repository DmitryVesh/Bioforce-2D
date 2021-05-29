using GameServer;
using System;
using System.Collections;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;

public enum GameState
{
    Waiting,
    Playing,
    Ended,
    Restarting,
    ShuttingDown
}
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get => instance; }
    private static GameStateManager instance;

    public GameState CurrentState { get; private set; } = GameState.Waiting;
    private void UpdateCurrentGameState(GameState state)
    {
        CurrentState = state;
        ServerSend.SendGameState(CurrentState, RemainingGameTime);
    }

    public Action<float> OnServerGameActivated { get; set; }
    public Action OnServerGameEnded { get; set; }

    
    public Action OnServerRestart { get; set; } //Each ClientServer will subscribe to this so the player model is destroyed, and is taken back to AskPlayerDetails 
    public Action OnServerStop { get; set; }
    public Action OnServerShutdown { get; set; }
    private Coroutine EndGameInCoroutine { get; set; }
    

    [SerializeField] private float GameTimeSeconds = 180f; //Game should last for 3min = 60sec/min * 3min = 180s
    private float StartGameTime { get; set; }
    private float FinGameTime { get; set; }
    public float RemainingGameTime
    {
        get
        {
            if (!CurrentState.Equals(GameState.Playing))
                return -1f;

            return FinGameTime - Time.fixedTime;
        }
    }

    [SerializeField] private float TimeForGameEndSeconds = 15f;
    [SerializeField] private float TimeForServerShutdownSeconds = 10f;

    internal void PlayerJoinedServer()
    {
        if (CurrentState != GameState.Waiting && CurrentState != GameState.Restarting) //Prevents more than 1 call
            return;
        
        StartGameTime = Time.fixedTime;
        FinGameTime = StartGameTime + GameTimeSeconds;

        OnServerGameActivated?.Invoke(RemainingGameTime);

        UpdateCurrentGameState(GameState.Playing);

        EndGameInCoroutine = StartCoroutine(EndGameIn(RemainingGameTime));
    }
    internal void TimeoutReset()
    {
        if (CurrentState != GameState.Playing)
            return;

        StopCoroutine(EndGameInCoroutine);
        RestartServerImmediately();

        Output.WriteLine(
            $"\n\t\t--------------------------------------------------" +
            $"\n\t\tRestarting GameServer due to no players present..." +
            $"\n\t\t--------------------------------------------------"
        );
    }
        
    private IEnumerator EndGameIn(float remainingGameTime)
    {
        yield return new WaitForSeconds(remainingGameTime);
        UpdateCurrentGameState(GameState.Ended);

        OnServerGameEnded?.Invoke();

        if (!Server.IsServerPermanent)
            StartCoroutine(CloseTheServer());
        else
            StartCoroutine(RestartServer());
    }

    private IEnumerator RestartServer()
    {
        yield return new WaitForSeconds(TimeForGameEndSeconds);
        RestartServerImmediately();
        Output.WriteLine(
            $"\n\t\t--------------------------------------------------" +
            $"\n\t\tRestarting GameServer because game has finished..." +
            $"\n\t\t--------------------------------------------------"
        );
    }
    private void RestartServerImmediately()
    {
        UpdateCurrentGameState(GameState.Restarting);
        OnServerRestart?.Invoke();
        UpdateCurrentGameState(GameState.Waiting);
    }

    private IEnumerator CloseTheServer()
    {
        yield return new WaitForSeconds(TimeForGameEndSeconds);
        //Server send to all players that the server is closing
        //Send to MainServer that the server is shutting down
        //Not accept any other connection attempts by other players
        UpdateCurrentGameState(GameState.ShuttingDown);
        OnServerStop?.Invoke();
        yield return new WaitForSeconds(TimeForServerShutdownSeconds);
        OnServerShutdown?.Invoke();
    }

    private void Awake()
    {
        Singleton.Init(ref instance, this);
    }

    
}
