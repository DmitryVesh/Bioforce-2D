using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealOverlay : Overlay
{
    protected virtual void ActivateOverlay(int currentHealth) =>
        Activate();

    protected override void SubscribeToActivationEvent() =>
        PlayerManager.OnPlayerRestoreHealth += ActivateOverlay;

    private void OnDestroy() =>
        PlayerManager.OnPlayerRestoreHealth -= ActivateOverlay;

}
