using UnityEngine;
using System;

public class LocalPlayerAnimations : NonLocalPlayerAnimations
{
    private IWalkingLocalPlayer LocalPlayerMoveInterface { get; set; } = null;

    protected override void Awake()
    {
        LocalPlayerMoveInterface = GetComponent<IWalkingLocalPlayer>();
        base.Awake();
    }

    public override void YAxisAnimations()
    {
        base.YAxisAnimations();
        if (LocalPlayerMoveInterface.HasJumped())
            Anim.SetTrigger("Jumped");
    }

}
