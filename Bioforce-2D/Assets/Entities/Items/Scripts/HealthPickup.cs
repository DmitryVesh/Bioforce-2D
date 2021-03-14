using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HealthPickup : PickupItem
{
    [SerializeField] private int RestoreMin;
    [SerializeField] private int RestoreMax;
    protected int Restore;

    private void Awake()
    {
        Restore = Random.Range(RestoreMin, RestoreMax + 1);
    }
}
