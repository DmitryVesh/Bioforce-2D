using UnityEngine;
using System;

public class LocalPlayerAnimations : NonLocalPlayerAnimations
{
    private LocalPlayerMovement MovementScript { get; set; } = null;

    protected override void Awake()
    {
        MovementScript = GetComponent<LocalPlayerMovement>();
        base.Awake();
    }

    public override void YAxisAnimations()
    {
        base.YAxisAnimations();
        if (MovementScript.HasJumped())
            Anim.SetTrigger("Jumped");
    }

}
