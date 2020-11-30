using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityWalking : MonoBehaviour, IWalking
{
    // Moving in X direction Data
    [SerializeField] protected float RunSpeed = 5; // Used as x velocity multiplier when running
    [SerializeField] protected float SprintSpeed = 7; // Used as x velocity multiplier when sprinting

    // Moving in Y direction Data
    [SerializeField] protected LayerMask WhatIsGround; // LayerMask used to determine what is considered ground for the player

    // References to other scripts
    protected GameObject ModelObject { get; set; } // The gameObject that holds the components of the entity
    protected Rigidbody2D rb { get; set; } // Used to add force in x and y directions, corresponding to input
    protected CapsuleCollider2D Hitbox { get; set; } // Used to determine if player is grounded

    // Variables used for Animations
    public bool Grounded { get; private set; } = false; // used to track if the entity has landed, or is the air
    protected bool CanMove { get; set; } = true;

    protected virtual void Awake()
    {
        ModelObject = transform.GetChild(0).gameObject;
        rb = ModelObject.GetComponent<Rigidbody2D>();
        Hitbox = ModelObject.GetComponent<CapsuleCollider2D>();
    }
    protected virtual void FixedUpdate()
    {
        if (!CanMove)
        {
            Grounded = true;
            return;
        }
        Grounded = IsGrounded();
    }
    private bool IsGrounded() // Raycast used to check if character is grounded, uses layemark whatIsGround
    {
        float extraHeight = 0.01f;
        // A ray is used to detect ground using what is defined in Inspector as ground using layers
        RaycastHit2D raycastHit = Physics2D.Raycast(Hitbox.bounds.center, Vector2.down, Hitbox.bounds.extents.y + extraHeight, WhatIsGround);
        //Color rayColor; // Debug color of the gizmo

        if (raycastHit)
        {
            //rayColor = Color.green;
            //Debug.DrawRay(hitbox.bounds.center, Vector2.down * (hitbox.bounds.extents.y + extraHeight), rayColor); // showing the gizmo line 
            return true;
        }
        else
        {
            //rayColor = Color.red;
            //Debug.DrawRay(hitbox.bounds.center, Vector2.down * (hitbox.bounds.extents.y + extraHeight), rayColor);
            return false;
        }
    }
    
    public bool GetGrounded()
    {
        return Grounded;
    }

    protected void FreezeMotion()
    {
        CanMove = false;
        rb.velocity.Set(0, 0);
        rb.Sleep();
    }
    protected void UnFreezeMotion()
    {
        CanMove = true;
        rb.WakeUp();
    }
}
