using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaBar : StatsBar
{
    private IWalkingLocalPlayer LocalPlayerMovementInterface { get; set; }

    private void Awake() =>
        LocalPlayerMovementInterface = GetComponentInParent<IWalkingLocalPlayer>();

    protected override float GetCurrent() =>
        LocalPlayerMovementInterface.GetCurrentStamina();

    protected override float GetMax() =>
        LocalPlayerMovementInterface.GetMaxStamina();
}
