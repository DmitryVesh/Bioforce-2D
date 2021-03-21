using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitOverlay : Overlay
{
    protected virtual void ActivateOverlay(int damage, int currentHealth) =>
        Activate();

    protected override void SubscribeToActivationEvent() =>
        PlayerManager.OnPlayerTookDamage += ActivateOverlay;

    
}
