﻿using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] public byte ID;
    [SerializeField] private string Username;
    public int MaxHealth { get; private set; }

    public Color PlayerColor { get; private set; }
    public Vector2 RespawnPosition { get; set; }

    public GameObject PlayerModelObject { get; private set; }
    [SerializeField] private TextMeshProUGUI UsernameText;

    // Delegate and event used to notify when movement stats are read from server
    public delegate void Float(float floatVal);
    public event Float OnPlayerMovementStatsChanged;

    public delegate void PlayerMovingStateChange(PlayerMovingState movingState);
    public event PlayerMovingStateChange OnPlayerMovingStateChange;

    public delegate void NoParams();
    public event NoParams OnPlayerRespawn;
    public delegate void PlayerDeath(TypeOfDeath typeOfDeath);
    public event PlayerDeath OnPlayerDeath;
    public static float RespawnTime { get; private set; } = 1.5f;
    public static float DeadTime { get; private set; } = 3;

    public delegate void PlayerChangeHealth(int currentHealth);
    public event PlayerChangeHealth OnPlayerTookDamage;
    public event PlayerChangeHealth OnPlayerRestoreHealth;

    public delegate void PositionAndRotation(Vector2 position, Quaternion rotation);
    public event PositionAndRotation OnPlayerShot;
    
    public event NoParams OnPlayerJumped;

    public delegate void Position(Vector2 position);
    public event Position OnPlayerPosition;
    public delegate void Rotation(Quaternion rotation);
    public event Rotation OnPlayerRotation;

    //public event PositionAndRotation OnArmPositionRotation;
    public event Position OnPlayerArmPosition;
    public event Rotation OnPlayerArmRotation;

    public delegate void Bool(bool boolean);

    public event Bool OnPlayerPaused;

    public delegate void Int(int integer);
    public event Int OnPlayerPickupBandage;
    public event Int OnPlayerPickupMedkit;

    public event Float OnPlayerInvincibility;
    public event Float OnPlayerPickupAdrenaline;

    public event NoParams OnPlayersBulletHitCollider;

    public event NoParams OnLocalPlayerHitAnother;

    public delegate void TripleInt(int integer1, int integer2, int integer3);
    public event TripleInt OnHeartBeatShouldPlay;

    public const float InvincibilityTimeAfterRespawning = 2f;

    internal void RestoreHealthEvent(int currentHealth) =>
        OnPlayerRestoreHealth?.Invoke(currentHealth);

    internal void OnHeartBeatShouldPlayEvent(int newHealth, int healthWhenHeartBeatShouldPlayMin, int healthWhenHeartBeatShouldPlayMax) =>
        OnHeartBeatShouldPlay?.Invoke(newHealth, healthWhenHeartBeatShouldPlayMin, healthWhenHeartBeatShouldPlayMax);

    public void PlayersBulletHitCollider() =>
        OnPlayersBulletHitCollider?.Invoke();

    internal void BandagePickup(int restoreHealth) =>
        OnPlayerPickupBandage?.Invoke(restoreHealth);
    internal void MedkitPickup(int restoreHealth) =>
        OnPlayerPickupMedkit?.Invoke(restoreHealth);

    public const float TimeBeforeStoppingFlashMultiplier = 0.875f;
    internal void AdrenalinePickup(float invincibilityTimeSec) 
    {
        if (invincibilityTimeSec <= 0)
            return;

        PlayerInvincible(invincibilityTimeSec);
        OnPlayerPickupAdrenaline?.Invoke(invincibilityTimeSec);
    }
    private void PlayerInvincible(float invincibilityTimeSec) =>
        OnPlayerInvincibility?.Invoke(invincibilityTimeSec);
    
    internal void PlayerInvincibleAfterRespawning() =>
        PlayerInvincible(RespawnTime + InvincibilityTimeAfterRespawning);

    public string GetUsername() =>
        Username;

    public void SetPlayerMovementStats(float runSpeed, float sprintSpeed) =>
        OnPlayerMovementStatsChanged?.Invoke(runSpeed);

    public void SetVelocityState(PlayerMovingState movingState) =>
        OnPlayerMovingStateChange?.Invoke(movingState);

    public void PlayerDied(TypeOfDeath typeOfDeath) =>
        OnPlayerDeath?.Invoke(typeOfDeath);

    public IEnumerator IsPlayerDeadUponSpawning(bool isDead)
    {
        if (isDead)
        {
            //Prevents calling before the Start methods which subscribe to the event
            yield return new WaitForSeconds(0.3f);
            OnPlayerDeath?.Invoke(TypeOfDeath.Bullet);
        }
    }
    public void PlayerRespawned()
    {
        OnPlayerRespawn?.Invoke();
    }

    public void CallOnPlayerJumpedEvent() =>
        OnPlayerJumped?.Invoke();

    public void Initialise(byte iD, string username, Color playerColor, int maxHealth) 
    {
        (ID, Username, MaxHealth) = (iD, username, maxHealth);
        SetUsername(playerColor);
    }

    public void SetRespawnPosition(Vector2 position)
    {
        PlayerModelObject.transform.position = position;
        SetPosition(position);
    }
    public void SetPosition(Vector2 position) =>
        OnPlayerPosition?.Invoke(position); 
    public void SetRotation(Quaternion rotation) =>
        OnPlayerRotation?.Invoke(rotation); 
        
    public void Disconnect() =>
        Destroy(gameObject);

    public void CallOnBulletShotEvent(Vector2 position, Quaternion rotation) =>
        OnPlayerShot?.Invoke(position, rotation);

    private void Awake()
    {
        PlayerModelObject = transform.GetChild(0).gameObject;
        SetUsername("");
    }

    private void SetUsername(Color playerColor)
    {
        UsernameText.text = Username;
        PlayerColor = playerColor;
        UsernameText.color = PlayerColor;
    }
    private void SetUsername(string username) =>
        UsernameText.text = username;

    public void TookDamage(int damage, int currentHealth)
    {
        DamageNumManager.Instance.Create(PlayerModelObject.transform.position, damage, PhysicsHelper.RandomBool());
        OnPlayerTookDamage?.Invoke(currentHealth);
    }

    public void CallLocalPlayerHitAnother() =>
        OnLocalPlayerHitAnother?.Invoke();

    internal void SetArmRotation(Quaternion rotation) =>
        OnPlayerArmRotation?.Invoke(rotation);
    internal void SetArmPosition(Vector2 position) =>
        OnPlayerArmPosition?.Invoke(position);
    internal void SetPlayerPaused(bool paused) =>
        OnPlayerPaused?.Invoke(paused);

}
