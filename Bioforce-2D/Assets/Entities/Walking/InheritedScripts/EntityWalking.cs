using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityWalking : MonoBehaviour, IWalking
{
    // Moving in X direction Data
    [SerializeField] protected float RunSpeed = 5; // Used as x velocity multiplier when running
    [SerializeField] protected float SprintSpeed = 7; // Used as x velocity multiplier when sprinting
    protected float CurrentSprintSpeed = 7;

    // Moving in Y direction Data
    [SerializeField] protected LayerMask WhatIsGround; // LayerMask used to determine what is considered ground for the player

    // References to other scripts
    protected GameObject ModelObject { get; set; } // The gameObject that holds the components of the entity
    protected Rigidbody2D RigidBody { get; set; } // Used to add force in x and y directions, corresponding to input
    protected CapsuleCollider2D Hitbox { get; set; } // Used to determine if player is grounded

    // Variables used for Animations
    public bool Grounded { get; private set; } = false; // used to track if the entity has landed, or is the air
    protected bool CanMove { get; set; } = true;

    //Vars used for Lerping movement of the entity
    protected bool ShouldLerpPosition { get; set; }
    protected bool ShouldLerpRotation { get; set; }

    protected Vector2 RealPosition { get; set; } // Set by walking entity when reading position and rotation
    protected Quaternion RealRotation { get; set; }
    protected Vector2 LastRealPosition { get; set; }
    protected Quaternion LastRealRotation { get; set; }

    protected float TimeStartLerpPosition { get; set; }
    protected float TimeStartLerpRotation { get; set; }

    protected virtual void Awake()
    {
        ModelObject = transform.GetChild(0).gameObject;
        RigidBody = ModelObject.GetComponent<Rigidbody2D>();
        Hitbox = ModelObject.GetComponent<CapsuleCollider2D>();
    }
    private void Start()
    {      
        RealPosition = ModelObject.transform.position;
        RealRotation = ModelObject.transform.rotation;
        ShouldLerpPosition = false;
        ShouldLerpRotation = false;
    }
    protected virtual void FixedUpdate()
    {
        
        if (!CanMove)
        {
            Grounded = true;
            return;
        }
        Grounded = IsGrounded();

        LerpPosition();
        LerpRotation();

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
        RigidBody.velocity.Set(0, 0);
        RigidBody.bodyType = RigidbodyType2D.Static;
    }
    protected virtual void UnFreezeMotion()
    {
        CanMove = true;
        RigidBody.bodyType = RigidbodyType2D.Kinematic;
    }

    public void LerpPosition()
    {
        if (ShouldLerpPosition)
        {
            float fractionLerp = (Time.time - TimeStartLerpPosition) / Time.fixedDeltaTime;
            ModelObject.transform.position = Vector2.Lerp(LastRealPosition, RealPosition, fractionLerp);

            if (fractionLerp > 1)
                ShouldLerpPosition = false;
        }
    }
    public void LerpRotation()
    {
        if (ShouldLerpRotation)
        {
            float fractionLerp = (Time.time - TimeStartLerpRotation) / Time.fixedDeltaTime;
            ModelObject.transform.rotation = Quaternion.Lerp(LastRealRotation, RealRotation, fractionLerp);
            
            if (fractionLerp > 1)
                ShouldLerpRotation = false;
        }
    }
}
