using UnityEngine;
using System;

public class LocalPlayerAnimations : NonLocalPlayerAnimations
{
    private IWalkingLocalPlayer LocalPlayerMoveInterface { get; set; } = null;
    private LocalPlayerGun LocalPlayerGun { get; set; }

    protected override void Awake()
    {
        LocalPlayerMoveInterface = GetComponent<IWalkingLocalPlayer>();
        LocalPlayerGun = GetComponent<LocalPlayerGun>();
        base.Awake();
    }

    public override void YAxisAnimations()
    {
        base.YAxisAnimations();
        if (LocalPlayerMoveInterface.HasJumped())
            Anim.SetTrigger("Jumped");
    }

    public override void FlipSprite()
    {
        base.FlipSprite();
        LocalPlayerGun.SetLookDir(FacingRight);
    }
}
