using System;
using UnityEngine;
using UnityEngine.Singleton;

public enum GameState
{
    waitingForAPlayerToJoin,
    activeGameInProcess
}
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get => instance; }
    private static GameStateManager instance;

    public GameState CurrentState { get; private set; } = GameState.waitingForAPlayerToJoin;
    public Action<float> OnServerActivated { get; set; }

    [SerializeField] private float GameTime = 240f; //Game should last for 4m = 240s
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

    internal void PlayerJoinedServer()
    {
        if (!CurrentState.Equals(GameState.waitingForAPlayerToJoin)) //Prevents more than 1 call
            return;

        CurrentState = GameState.activeGameInProcess;
        StartGameTime = Time.fixedTime;
        FinGameTime = StartGameTime + GameTime;

        OnServerActivated?.Invoke(RemainingGameTime);
    }

    private void Awake()
    {
        Singleton.Init(ref instance, this);
    }
}
