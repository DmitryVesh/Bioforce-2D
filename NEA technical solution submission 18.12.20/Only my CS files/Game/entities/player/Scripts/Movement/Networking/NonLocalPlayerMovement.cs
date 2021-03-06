﻿using System.Collections;
using System.Collections.Generic;
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

        PlayerCanMoveAndCanBeHit();
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
    }
    
    private void OnDestroy()
    {
        PlayerManager.OnPlayerMovementStatsChanged -= ChangedPlayerMovementStats;
        PlayerManager.OnPlayerSpeedXChanged -= ChangedSpeedX;
        PlayerManager.OnPlayerDeath -= PlayerCantMoveAndCantBeHit;
        PlayerManager.OnPlayerRespawn -= PlayerCanMoveAndCanBeHit;
    }
}
