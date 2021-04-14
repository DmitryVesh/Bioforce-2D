using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdrenalinePickup : PickupItem
{
    [HideInInspector] public float InvincibilityTime { get; set; } = 0;
    public override void PlayerPickedUp(PlayerManager player)
    {
        base.PlayerPickedUp(player);
        player.AdrenalinePickup(InvincibilityTime);
    }
}
