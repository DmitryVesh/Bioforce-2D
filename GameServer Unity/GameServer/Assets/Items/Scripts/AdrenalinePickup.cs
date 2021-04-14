using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdrenalinePickup : PickupItem
{
    public float InvincibilityTime = 10f;
    protected override void PickedUp(PlayerServer player)
    {
        base.PickedUp(player);
        player.PickedUpAdrenaline(InvincibilityTime);
    }
}
