using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerHealth : NonLocalPlayerHealth
{
    [SerializeField] private int HealthWhenHeartBeatShouldPlayMin;
    [SerializeField] private int HealthWhenHeartBeatShouldPlayMax;

    private bool Invincible { get; set; } = false;
    private Coroutine InvincibleCoroutine;

    public override void TakeDamage(int damage, byte bulletOwnerID)
    {
        if (CantGetHit || Invincible)
            return;

        int newHealth = CurrentHealth - damage;
        PlayerManager.TookDamage(damage, newHealth);
        ClientSend.TookDamage(damage, CurrentHealth, bulletOwnerID);

        if (newHealth <= HealthWhenHeartBeatShouldPlayMin)
            PlayerManager.OnHeartBeatShouldPlayEvent(newHealth, HealthWhenHeartBeatShouldPlayMin, HealthWhenHeartBeatShouldPlayMax);

        if (newHealth <= 0)
            Die(bulletOwnerID);
    }
    
    protected override void Start()
    {
        base.Start();
        PlayerManager.OnPlayerInvincibility += PlayerIsInvincible;
    }

    private void PlayerIsInvincible(float invincibleTime)
    {
        if (!(InvincibleCoroutine is null))
            StopCoroutine(InvincibleCoroutine);

        InvincibleCoroutine = StartCoroutine(EnableInvinciblity(invincibleTime));
    }
    private IEnumerator EnableInvinciblity(float invincibleTime)
    {
        Invincible = true;
        yield return new WaitForSeconds(invincibleTime);
        Invincible = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        PlayerManager.OnPlayerInvincibility -= PlayerIsInvincible;
    }

    protected override void PickupHealth(int restoreHealth)
    {
        base.PickupHealth(restoreHealth);
        PlayerManager.OnHeartBeatShouldPlayEvent(CurrentHealth, HealthWhenHeartBeatShouldPlayMin, HealthWhenHeartBeatShouldPlayMax);
    }

    protected override IEnumerator WaitBeforeRespawning()
    {
        yield return base.WaitBeforeRespawning();
        PlayerIsInvincible(PlayerManager.RespawnTime + PlayerManager.InvincibilityTimeAfterRespawning);
    }

    public override void Respawn()
    {
        base.Respawn();
        Invoke("SetCantGetHitFalse", PlayerManager.RespawnTime);
        GameManager.Instance.PlayerRespawned(OwnerClientID);
    }
}
