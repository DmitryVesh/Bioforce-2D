using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HealthPickup : PickupItem
{
    [SerializeField] private int RestoreMin;
    [SerializeField] private int RestoreMax;
    public int Restore { get; private set; }

    private void Awake()
    {
        Restore = Random.Range(RestoreMin, RestoreMax + 1);
    }

    protected override void PickedUp(PlayerServer player)
    {
        base.PickedUp(player);
        player.RestoreHealth(Restore);
    }
}
