﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonLocalPlayerHealth : MonoBehaviour, IHealth
{
    [SerializeField] private int MaxHealth = 50;
    [SerializeField] private int CurrentHealth = 0;

    //Set to default of -1, which will mean that the object will take damage from any bullet
    public int OwnerClientID { get; private set; } = -1;

    private PlayerManager PlayerManager { get; set; }

    public float GetMaxHealth() =>
        MaxHealth;
    public float GetCurrentHealth() =>
        CurrentHealth;

    public void TookDamage(int damage, int currentHealth)
    {
        //TODO: display hit effect
        //TODO: display damage hit numbers
        CurrentHealth = currentHealth;
        Debug.Log($"took damage: {OwnerClientID}");
    }
    public void TakeDamage(int damage, int bulletOwnerID)
    {
        CurrentHealth -= damage;
        //TODO: display hit effect
        ClientSend.TookDamage(damage, CurrentHealth);
        if (CurrentHealth <= 0) 
            Die(bulletOwnerID);
    }
    public void SetOwnerClientID(int iD) =>
        OwnerClientID = iD;

    public int GetOwnerClientID() => 
        OwnerClientID;

    public void Die(int bulletOwnerID)
    {
        GameManager.Instance.PlayerDied(OwnerClientID, bulletOwnerID);
        StartCoroutine(WaitBeforeRespawning());
    }
    public void Respawn()
    {
        ResetHealth();
        GameManager.Instance.PlayerRespawned(OwnerClientID);
    }
    public void FallDie()
    {
        GameManager.Instance.PlayerDied(OwnerClientID, OwnerClientID);
        StartCoroutine(WaitBeforeRespawning());
    }

    //TODO: 9000 maybe call Respawn instead of ResetHealth()????????????????????
    private void Start()
    {
        ResetHealth();
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];
        PlayerManager.OnPlayerTookDamage += TookDamage;
        PlayerManager.OnPlayerRespawn += ResetHealth;
    }
    private IEnumerator WaitBeforeRespawning()
    {
        yield return new WaitForSeconds(PlayerManager.DeadTime);
        Respawn();
    }
    protected void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }
}
