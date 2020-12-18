using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : StatsBar
{
    private IHealth HealthInterface { get; set; }

    private void Awake() =>
        HealthInterface = GetComponentInParent<IHealth>();

    protected override float GetCurrent() =>
        HealthInterface.GetCurrentHealth();

    protected override float GetMax() =>
        HealthInterface.GetMaxHealth();
}
