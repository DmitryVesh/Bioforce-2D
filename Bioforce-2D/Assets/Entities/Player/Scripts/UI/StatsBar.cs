using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatsBar : MonoBehaviour
{
    private Bar[] Bars { get; set; }

    protected abstract float GetMax();
    protected abstract float GetCurrent();

    private void Start()
    {
        Bars = transform.GetComponentsInChildren<Bar>();
        foreach (Bar bar in Bars)
            bar.SetMaxBarValue(0, GetMax());
    }

    private void FixedUpdate()
    {
        foreach (Bar bar in Bars)
            bar.SetCurrentBarValue(GetCurrent());

    }
}
