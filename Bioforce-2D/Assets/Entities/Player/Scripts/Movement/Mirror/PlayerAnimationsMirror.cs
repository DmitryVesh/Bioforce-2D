using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationsMirror : NetworkBehaviour
{
    [SerializeField] Animator Animator;

    //Set by PlayerMovementMirror
    bool Sprinting;
    float MoveSpeedX;
    bool Jumped;
    bool Grounded;

    bool LastGrounded;

    internal void SetMovementData(bool sprinting, float moveSpeedX, bool jumped, bool grounded) =>
        (Sprinting, MoveSpeedX, Jumped, Grounded) = (sprinting, moveSpeedX, jumped, grounded);

    private void FixedUpdate()
    {
        XAxisAnimations();
        YAxisAnimations();
    }


    private void XAxisAnimations()
    {
        if (MoveSpeedX < 0) //Moving left
            MoveSpeedX = -MoveSpeedX; // Adjusting the horizontal speed to a positive value if moving to left

        bool moving = MoveSpeedX != 0;
        Animator.SetBool("Moving", moving);

        Animator.SetBool("Sprinting", Sprinting);

        //TODO: Add Footstep Particles
        //HandleFootstepParticles(moving, sprinting);
    }
    private void YAxisAnimations()
    {
        //TODO: Add jump and land impact particles
        
        if (Jumped) {
            Animator.SetTrigger("Jumped");
            //ImpactOnFlootParticlesSmall.Play();
            LastGrounded = false;
            return;
        }

        Animator.SetBool("Grounded", Grounded);


        if (Grounded == LastGrounded)
            return;

        //if (Grounded) //Just landed
        //    ImpactOnFlootParticles.Play();
        LastGrounded = Grounded;
    }
}
