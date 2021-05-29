using System.Collections;
using UnityEngine;

public enum PlayerMovingState
{
    idleLeft,
    idleRight,

    runningLeft,
    runningRight,

    sprintingLeft,
    sprintingRight
}

public class NonLocalPlayerMovement : EntityWalking, IWalkingPlayer
{
    protected float SpeedX { get; set; } // Used to store the input in x direction, as well as in PlayerAnimations
    protected byte OwnerClientID { get; set; } = 255;
    protected PlayerManager PlayerManager { get; private set; } = null;
    protected PlayerMovingState CurrentMovingState { get; set; }

    public float GetRunSpeed()
    {
        return RunSpeed;
    }
    public float GetSpeedX()
    {
        return SpeedX;
    }

    public void SetOwnerClientID(byte iD)
    {
        OwnerClientID = iD;
    }

    protected override void Awake()
    {
        base.Awake();

        PlayerManager = GetComponent<PlayerManager>(); //Can do in awake because this component is on the same GameObject

        PlayerManager.OnPlayerMovementStatsChanged += ChangedPlayerMovementStats; //Subscribe to the PlayerMovementStatsChanged event, so can change runSpeed
        PlayerManager.OnPlayerMovingStateChange += ChangedSpeedX;
        PlayerManager.OnPlayerDeath += PlayerCantMoveAndCantBeHit;
        PlayerManager.OnPlayerRespawn += PlayerCanMoveAndCanBeHit;

        PlayerManager.OnPlayerPosition += PlayerPosition;
        PlayerManager.OnPlayerRotation += PlayerRotation;
    }
    protected virtual void Start()
    {
        //PlayerManager = GameManager.PlayerDictionary[OwnerClientID];

        //PlayerManager.OnPlayerMovementStatsChanged += ChangedPlayerMovementStats; //Subscribe to the PlayerMovementStatsChanged event, so can change runSpeed
        //PlayerManager.OnPlayerMovingStateChange += ChangedSpeedX;
        //PlayerManager.OnPlayerDeath += PlayerCantMoveAndCantBeHit;
        //PlayerManager.OnPlayerRespawn += PlayerCanMoveAndCanBeHit;

        //PlayerManager.OnPlayerPosition += PlayerPosition;
        //PlayerManager.OnPlayerRotation += PlayerRotation;

        GameManager.Instance.OnLostConnectionEvent += PlayerCantMoveWhenPaused;

        PlayerCanMoveAndCanBeHit();
    }

    private void PlayerCantMoveWhenPaused(bool pause)
    {
        if (pause)
            FreezeMotion();
        else
            UnFreezeMotion();
    }

    private void PlayerPosition(Vector2 position)
    {
        LastRealPosition = RealPosition;
        RealPosition = position;
        if (RealPosition != (Vector2)ModelObject.transform.position)
            ShouldLerpPosition = true;
        TimeStartLerpPosition = Time.time;
    }
    private void PlayerRotation(Quaternion rotation)
    {
        LastRealRotation = RealRotation;
        RealRotation = rotation;
        if (RealRotation.eulerAngles != ModelObject.transform.rotation.eulerAngles)
            ShouldLerpRotation = true;
        TimeStartLerpRotation = Time.time;
    }
    

    private void ChangedPlayerMovementStats(float runSpeed)
    {
        RunSpeed = runSpeed;
    }
    private void ChangedSpeedX(PlayerMovingState movingState)
    {
        CurrentMovingState = movingState;

        float speedX;
        switch (movingState)
        {
            case PlayerMovingState.idleLeft:
            case PlayerMovingState.idleRight:
                speedX = 0;
                break;
            case PlayerMovingState.runningLeft:
                speedX = -RunSpeed;
                break;
            case PlayerMovingState.runningRight:
                speedX = RunSpeed;
                break;
            case PlayerMovingState.sprintingLeft:
                speedX = -SprintSpeed;
                break;
            default: //same as -> case PlayerMovingState.sprintingRight:
                speedX = SprintSpeed;
                break;
        }
        SpeedX = speedX;
    }
    private void PlayerCantMoveAndCantBeHit(TypeOfDeath typeOfDeath)
    {
        Hitbox.enabled = false;
        RigidBody.simulated = false;
        FreezeMotion();
    }
    protected virtual void PlayerCanMoveAndCanBeHit()
    {
        StartCoroutine(UnFreezeMotionAndHitBoxAfterRespawnAnimation());
    }
    protected virtual IEnumerator UnFreezeMotionAndHitBoxAfterRespawnAnimation()
    {
        yield return new WaitForSeconds(PlayerManager.RespawnTime);
        UnFreezeMotion();
        Hitbox.enabled = true;
        RigidBody.simulated = true;
    }
    
    private void OnDestroy()
    {
        PlayerManager.OnPlayerMovementStatsChanged -= ChangedPlayerMovementStats;
        PlayerManager.OnPlayerMovingStateChange -= ChangedSpeedX;
        PlayerManager.OnPlayerDeath -= PlayerCantMoveAndCantBeHit;
        PlayerManager.OnPlayerRespawn -= PlayerCanMoveAndCanBeHit;

        PlayerManager.OnPlayerPosition -= PlayerPosition;
        PlayerManager.OnPlayerRotation -= PlayerRotation;

        GameManager.Instance.OnLostConnectionEvent -= PlayerCantMoveWhenPaused;
    }
}
