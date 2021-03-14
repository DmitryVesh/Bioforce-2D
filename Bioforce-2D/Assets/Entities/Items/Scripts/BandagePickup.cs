using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BandagePickup : HealthPickup
{
    protected override void PlayerPickedUp(PlayerManager player)
    {
        base.PlayerPickedUp(player);
        player.BandagePickup(Restore);
    }
}
