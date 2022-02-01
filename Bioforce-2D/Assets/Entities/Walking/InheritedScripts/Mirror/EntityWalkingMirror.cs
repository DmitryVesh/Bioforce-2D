using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityWalkingMirror : NetworkBehaviour, IWalkingMirror
{
    // Moving in X direction Data
    [SerializeField] protected float RunSpeed = 5; // Used as x velocity multiplier when running
    [SerializeField] protected float SprintSpeed = 7; // Used as x velocity multiplier when sprinting
    protected float CurrentSprintSpeed = 7;

    // Moving in Y direction Data
    [SerializeField] protected LayerMask WhatIsGround; // LayerMask used to determine what is considered ground for the player

    // References to other scripts
    [SerializeField] protected GameObject ModelObject; // The gameObject that holds the components of the entity
    [SerializeField] protected Rigidbody2D RigidBody; // Used to add force in x and y directions, corresponding to input
    [SerializeField] protected CapsuleCollider2D Hitbox; // Used to determine if player is grounded

    // Variables used for Animations
    public bool Grounded { get; private set; } = false; // used to track if the entity has landed, or is the air
    protected bool CanMove { get; set; } = true;


    protected virtual void Awake()
    {
        
    }
    protected virtual void Start()
    {

    }

    [ServerCallback]
    protected virtual void Update()
    {
        if (!CanMove)
        {
            Grounded = true;
            return;
        }
        Grounded = IsGrounded();
    }
    
    protected virtual void FixedUpdate()
    {       
        
    }

    private bool IsGrounded() // Raycast used to check if character is grounded, uses layemark whatIsGround
    {
        float extraHeight = 0.01f;
        // A ray is used to detect ground using what is defined in Inspector as ground using layers
        RaycastHit2D raycastHit = Physics2D.Raycast(Hitbox.bounds.center, Vector2.down, Hitbox.bounds.extents.y + extraHeight, WhatIsGround);
        Color rayColor; // Debug color of the gizmo

        if (raycastHit)
        {
            rayColor = Color.green;
            Debug.DrawRay(Hitbox.bounds.center, Vector2.down * (Hitbox.bounds.extents.y + extraHeight), rayColor); // showing the gizmo line 
            return true;
        }
        else
        {
            rayColor = Color.red;
            Debug.DrawRay(Hitbox.bounds.center, Vector2.down * (Hitbox.bounds.extents.y + extraHeight), rayColor);
            return false;
        }
    }
    public bool GetGrounded() => Grounded;

    protected void FreezeMotion()
    {
        CanMove = false;
        RigidBody.velocity.Set(0, 0);
        RigidBody.bodyType = RigidbodyType2D.Static;
    }
    protected virtual void UnFreezeMotion()
    {
        CanMove = true;
        RigidBody.bodyType = RigidbodyType2D.Kinematic;
    }
}
