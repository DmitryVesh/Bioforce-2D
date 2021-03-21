using System.Collections;
using UnityEngine;

public class NonLocalPlayerMovement : EntityWalking, IWalkingPlayer
{
    protected float SpeedX { get; set; } // Used to store the input in x direction, as well as in PlayerAnimations
    protected int OwnerClientID { get; set; } = -1;
    protected PlayerManager PlayerManager { get; set; } = null;

    public float GetRunSpeed()
    {
        return RunSpeed;
    }
    public float GetSpeedX()
    {
        return SpeedX;
    }

    public void SetOwnerClientID(int iD)
    {
        OwnerClientID = iD;
    }

    protected virtual void Start()
    {
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];

        PlayerManager.OnPlayerMovementStatsChanged += ChangedPlayerMovementStats; //Subscribe to the PlayerMovementStatsChanged event, so can change runSpeed
        PlayerManager.OnPlayerSpeedXChanged += ChangedSpeedX;
        PlayerManager.OnPlayerDeath += PlayerCantMoveAndCantBeHit;
        PlayerManager.OnPlayerRespawn += PlayerCanMoveAndCanBeHit;

        PlayerManager.OnPlayerPosition += PlayerPosition;
        PlayerManager.OnPlayerRotation += PlayerRotation;

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
    private void ChangedSpeedX(float speedX)
    {
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
        PlayerManager.OnPlayerSpeedXChanged -= ChangedSpeedX;
        PlayerManager.OnPlayerDeath -= PlayerCantMoveAndCantBeHit;
        PlayerManager.OnPlayerRespawn -= PlayerCanMoveAndCanBeHit;
    }
}
