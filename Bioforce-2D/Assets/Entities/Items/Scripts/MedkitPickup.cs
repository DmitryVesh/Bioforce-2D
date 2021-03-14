using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedkitPickup : HealthPickup
{
    protected override void PlayerPickedUp(PlayerManager player)
    {
        base.PlayerPickedUp(player);
        player.MedkitPickup(Restore);
    }
}
