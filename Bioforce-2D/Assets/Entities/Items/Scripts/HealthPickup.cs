using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HealthPickup : PickupItem
{
    [HideInInspector] public int Restore { get; set; } = 0;
}
