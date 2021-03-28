using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerHealth : NonLocalPlayerHealth
{
    [SerializeField] private int HealthWhenHeartBeatShouldPlayMin;
    [SerializeField] private int HealthWhenHeartBeatShouldPlayMax;

    public override void TakeDamage(int damage, int bulletOwnerID)
    {
        if (CantGetHit)
            return;

        int newHealth = CurrentHealth - damage;
        PlayerManager.TookDamage(damage, newHealth);
        ClientSend.TookDamage(damage, CurrentHealth, bulletOwnerID);

        if (newHealth <= HealthWhenHeartBeatShouldPlayMin)
            PlayerManager.OnHeartBeatShouldPlayEvent(newHealth, HealthWhenHeartBeatShouldPlayMin, HealthWhenHeartBeatShouldPlayMax);

        if (newHealth <= 0)
            Die(bulletOwnerID);
    }
}
