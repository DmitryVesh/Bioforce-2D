using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdrenalinePickup : PickupItem
{
    private float InvincibilityTimeSec { get; set; } = 0;
    public override void PlayerPickedUp(PlayerManager player)
    {
        base.PlayerPickedUp(player);
        player.AdrenalinePickup(InvincibilityTimeSec);
    }

    internal void SetInvincibilityTime(float invincibilityTimeSec) =>
        InvincibilityTimeSec = invincibilityTimeSec;
}
