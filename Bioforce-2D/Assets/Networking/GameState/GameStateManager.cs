using System;

public enum GameState
{
    //If adding states, add to the GameStateManager.ServerStates the relevant string representation
    Waiting,
    Playing,
    Ended,
    Restarting,
    ShuttingDown
}
public static class GameStateManager
{
    //public static readonly string[] ServerStates = { "Waiting", "Playing", "Ended", "Restarting", "Shutting down" };

    public static GameState CurrentState { get; private set; } = GameState.Waiting;
    public static float RemainingGameTime { get; private set; }

    public static Action<float> GameActivated { get; set; }
    public static Action WaitingForGame { get; set; }
    //Need to implement
    public static Action GameEnded { get; set; }
    public static Action GameRestarting { get; set; }
    public static Action ServerShuttingDown { get; set; }
    //

    public static void ReadGameState(GameState currentState, float remainingGameTime)
    {
        CurrentState = currentState;
        RemainingGameTime = remainingGameTime;

        switch (CurrentState)
        {
            case GameState.Waiting:
                WaitingForGame?.Invoke();
                break;
            case GameState.Playing:
                GameActivated?.Invoke(RemainingGameTime);
                break;
            case GameState.Ended:
                GameEnded?.Invoke();
                break;
            case GameState.Restarting:
                GameRestarting?.Invoke();
                break;
            case GameState.ShuttingDown:
                ServerShuttingDown?.Invoke();
                break;
        }
    }
}
