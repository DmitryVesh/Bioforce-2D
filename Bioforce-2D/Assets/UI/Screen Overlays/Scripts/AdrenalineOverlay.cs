using UnityEngine;

public class AdrenalineOverlay : Overlay
{
    [SerializeField] [Range(0.1f, 1)] private float TimeBeforeClearingCutMultiplier = 0.75f;
    protected virtual void ActivateOverlay(float InvincibilityTime)
    {
        TimeBeforeClearing = InvincibilityTime * TimeBeforeClearingCutMultiplier;
        Activate();
    }

    protected override void SubscribeToActivationEvent()
    {
        PlayerManager.OnPlayerPickupAdrenaline += ActivateOverlay;
        PlayerManager.OnPlayerRespawn += DeactivateOverlay;
    }

    private void OnDestroy()
    {
        PlayerManager.OnPlayerPickupAdrenaline -= ActivateOverlay;
        PlayerManager.OnPlayerRespawn -= DeactivateOverlay;
    }

}
