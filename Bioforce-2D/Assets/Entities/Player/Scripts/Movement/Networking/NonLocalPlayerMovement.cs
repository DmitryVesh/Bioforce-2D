using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonLocalPlayerMovement : EntityWalking, IWalkingPlayer
{
    protected float SpeedX { get; set; } // Used to store the input in x direction, as well as in PlayerAnimations
    private int OwnerClientID { get; set; } = -1;
    private PlayerManager PlayerManager { get; set; } = null;

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

    private void Start()
    {
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];

        PlayerManager.OnPlayerMovementStatsChanged += ChangedPlayerMovementStats; //Subscribe to the PlayerMovementStatsChanged event, so can change runSpeed
        PlayerManager.OnPlayerSpeedXChanged += ChangedSpeedX;
        PlayerManager.OnPlayerDeath += PlayerCantMoveAndCantBeHit;
        PlayerManager.OnPlayerRespawn += PlayerCanMoveAndCanBeHit;
    }

    private void ChangedPlayerMovementStats(float runSpeed)
    {
        RunSpeed = runSpeed;
    }
    private void ChangedSpeedX(float speedX)
    {
        SpeedX = speedX;
    }
    private void PlayerCantMoveAndCantBeHit()
    {
        if (CanMove)
        {
            Hitbox.enabled = false;
            FreezeMotion();
        }
    }
    private void PlayerCanMoveAndCanBeHit()
    {
        StartCoroutine(UnFreezeMotionAndHitBoxAfterRespawnAnimation());
    }
    private IEnumerator UnFreezeMotionAndHitBoxAfterRespawnAnimation()
    {
        yield return new WaitForSeconds(PlayerManager.RespawnTime);
        UnFreezeMotion();
        Hitbox.enabled = true;
    }
    
    private void OnDestroy()
    {
        PlayerManager.OnPlayerMovementStatsChanged -= ChangedPlayerMovementStats;
        PlayerManager.OnPlayerSpeedXChanged -= ChangedSpeedX;
        PlayerManager.OnPlayerDeath -= PlayerCantMoveAndCantBeHit;
        PlayerManager.OnPlayerRespawn -= PlayerCanMoveAndCanBeHit;
    }
}
