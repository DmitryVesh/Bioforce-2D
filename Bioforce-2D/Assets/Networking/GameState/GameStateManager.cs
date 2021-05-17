using System;

public enum GameState
{
    waitingForAPlayerToJoin,
    activeGameInProcess,
    gameEnded,
    gameRestarting,
    serverShuttingDown
}
public static class GameStateManager
{
    public static GameState CurrentState { get; private set; } = GameState.waitingForAPlayerToJoin;
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
            case GameState.waitingForAPlayerToJoin:
                WaitingForGame?.Invoke();
                break;
            case GameState.activeGameInProcess:
                GameActivated?.Invoke(RemainingGameTime);
                break;
            case GameState.gameEnded:
                GameEnded?.Invoke();
                break;
            case GameState.gameRestarting:
                GameRestarting?.Invoke();
                break;
            case GameState.serverShuttingDown:
                ServerShuttingDown?.Invoke();
                break;
        }
    }
}
