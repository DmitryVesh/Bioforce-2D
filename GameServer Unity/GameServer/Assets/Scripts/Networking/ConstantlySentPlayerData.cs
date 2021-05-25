using UnityEngine;

public class ConstantlySentPlayerData
{
    public bool UseMoveState { get; private set; } = false;
    public byte MoveState { get; private set; }

    public bool UsePlayerPosition { get; private set; } = false;
    public Vector2 PlayerPosition { get; private set; }

    public bool UseArmPosition { get; private set; } = false;
    public Vector2 ArmPosition { get; private set; }

    public bool UseArmRotation { get; private set; } = false;
    public Quaternion ArmRotation { get; private set; }
    
    public bool UsePingMS { get; private set; } = false;
    public ushort PingMS { get; private set; }
    

    //If adding fields need to Change -> Reset(), SetAll(), HasAnyData(), Clone()
    //Add a method SetX(T value) where Value is T, where UseValue is bool =>
    //  (Value, UseValue) = (value, true)

    public void Reset()
    {
        UseMoveState = false;
        UsePlayerPosition = false;
        UseArmPosition = false;
        UseArmRotation = false;
        UsePingMS = false;
    }
    public void SetAll(
        bool useMoveState, byte moveState,
        bool usePlayerPosition, Vector2 playerPosition,
        bool useArmPosition, Vector2 armPosition,
        bool useArmRotation, Quaternion armRotation,
        bool usePing, ushort pingMS)
    {
        UseMoveState = useMoveState;
        MoveState = moveState;

        UsePlayerPosition = usePlayerPosition;
        PlayerPosition = playerPosition;

        UseArmPosition = useArmPosition;
        ArmPosition = armPosition;

        UseArmRotation = useArmRotation;
        ArmRotation = armRotation;
        
        UsePingMS = usePing;
        PingMS = pingMS;
    }
    public bool HasAnyData() =>
        UseMoveState || UsePlayerPosition || UseArmPosition || UseArmRotation || UsePingMS;

    internal ConstantlySentPlayerData Clone()
    {
        ConstantlySentPlayerData clone = new ConstantlySentPlayerData();
        clone.SetAll(
            UseMoveState, MoveState,
            UsePlayerPosition, PlayerPosition,
            UseArmPosition, ArmPosition,
            UseArmRotation, ArmRotation,
            UsePingMS, PingMS);
        return clone;
    }

    public void SetMoveState(byte moveState) =>
        (MoveState, UseMoveState) = (moveState, true);
    public void SetPlayerPosition(Vector2 playerPositon) =>
        (PlayerPosition, UsePlayerPosition) = (playerPositon, true);
    public void SetArmPosition(Vector2 armPosition) =>
        (ArmPosition, UseArmPosition) = (armPosition, true);
    public void SetArmRotation(Quaternion armRotation) =>
        (ArmRotation, UseArmRotation) = (armRotation, true);
    public void SetPing(ushort pingMS) =>
        (PingMS, UsePingMS) = (pingMS, true);
}
