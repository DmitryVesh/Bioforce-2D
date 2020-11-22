using UnityEngine;
using System;

public class LocalPlayerAnimations : NonLocalPlayerAnimations
{
    private PlayerMovementNetworking MovementScript { get; set; } = null;

    protected override void Awake()
    {
        IsLocalPlayer = true;
        base.Awake();
        MovementScript = GetComponent<PlayerMovementNetworking>();
    }
    protected override void FixedUpdate()
    {
        SpeedX = MovementScript.SpeedX;
        base.FixedUpdate();
    }

    protected override void YaxisAnimations()
    {
        Anim.SetBool("Grounded", MovementScript.Grounded);
        if (MovementScript.HasJumped())
            Anim.SetTrigger("Jumped");
    }
}
