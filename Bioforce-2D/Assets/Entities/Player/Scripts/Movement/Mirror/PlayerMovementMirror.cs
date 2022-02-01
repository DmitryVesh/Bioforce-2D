using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementMirror : EntityWalkingMirror
{
    [SerializeField] PlayerInputMirror PlayerInput;
    [SerializeField] PlayerAnimationsMirror PlayerAnimations;

    // Stamina Data
    [SerializeField] private float MaxStamina = 5; // Used as time limit for how much a player can sprint
    [SerializeField] private float RateStaminaLoss = 1; // Used as value for the rate of stamina lost when sprinting
    [SerializeField] private float RateStaminaRegen = 0.5f; // Used as value for the rate of stamina regenerated when not sprinting
    private float currentStamina; // Used as the stamina left to the player

    // Moving in Y direction Data
    private const string PlatformLayerName = "Platform"; // LayerMask used to determine what is a platform, so can go through that layer
    private int PlatformLayer;

    [SerializeField] private float JumpForce = 10; // the force applied to player when they jump
    [SerializeField] private int NumberOfExtraJumps = 1; // the number of jumps given to the player
    [SerializeField] private int currentNumJumps; // number of jumps left

    [SerializeField] private float FJumpPressedRememberTime = 0.2f; // Timer used to "remember" when the last time a jump key was pressed, improving feeling of responsiveness
                                                                    // When player presses a jump key just above ground, the jump will still occur when player lands, if within timer limit
                                                                    // Helps the player not feel stupid
    private float fJumpPressedRemember = 0; // Holds current jump remember timer time
    [SerializeField] private float FGroundedRememberTime = 0.2f;    // Timer, used to "remember" that the player has been grounded,
                                                                    // Helps player to still be able to jump with >1 jump, if just started falling off an edge
                                                                    // Helps the player not feel stupid
    private float fGroundedRemember = 0; // Holds current ground remember timer time
    [SerializeField] private float FCutJumpHeight = 0.45f; // How much up velocity is cut when releasing the jump key, getting mario jump control effect
    [SerializeField] private float GroundCheckTimer = 0.15f; // Timer used to check for ground, should't be all the time, as it CPU taxing to draw rays
                                                             // Also allows the player to be grounded for longer 
    private float groundTimer = 0; // Holds current ground check timer time


    protected override void Awake()
    {
        base.Awake();

        PlatformLayer = LayerMask.NameToLayer(PlatformLayerName);
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void UnFreezeMotion()
    {
        base.UnFreezeMotion();
    }

    [ServerCallback]
    protected override void Update()
    {
        base.Update();

        (bool sprinting, float moveSpeedX) = MovementX();
        bool jumped = MovementY();

        PlayerAnimations.SetMovementData(sprinting, moveSpeedX, jumped, Grounded);
        RpcUpdatePlayerMovementData(sprinting, moveSpeedX, jumped, Grounded, currentStamina);
    }

    [ClientRpc]
    private void RpcUpdatePlayerMovementData(bool sprinting, float moveSpeedX, bool jumped, bool grounded, float currentStamina)
    {
        PlayerAnimations.SetMovementData(sprinting, moveSpeedX, jumped, grounded);
        //TODO: Set Stamina UI with currentStamina
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private (bool sprinting, float moveSpeedX) MovementX() // Calculates SpeedX that will be applied to player, in response to x axis inputs
    {
        float moveInputX = PlayerInput.MoveInputX;      
        bool pressedJump = PlayerInput.JumpPressed;
        bool moving = moveInputX != 0;
        
        //Need for Animations
        bool Sprinting = false;
        float currentMoveSpeedX;

        if (pressedJump && moving) // Player is moving and trying to sprint
        {
            if (currentStamina > 0) // Is stamina left
            {
                currentMoveSpeedX = moveInputX * CurrentSprintSpeed; // Set moving speed to sprint speed
                currentStamina -= RateStaminaLoss * Time.deltaTime; // Lossing stamina
                Sprinting = true;
            }
            else
            {
                //TODO: Give a player cooldown after fully exhausting the stamina bar
                currentMoveSpeedX = moveInputX * RunSpeed; // Set moving speed to normal running speed
            }
        }
        else // Player is either not moving, moving, or is holding shift without moving  
        {
            currentMoveSpeedX = moveInputX * RunSpeed; // Set moving speed to normal running speed
            float regenStamina = ((currentStamina < MaxStamina) ? 1 : 0) * (RateStaminaRegen * Time.deltaTime); // determines a regen value either: 0 if currentStamina >= MaxStamina, else regen rate
            currentStamina += regenStamina; // Regenerating stamina
        }

        Vector2 velocity = new Vector2(currentMoveSpeedX, RigidBody.velocity.y);
        if (CanMove)
            RigidBody.velocity = velocity; // Applying movement in x direction, SpeedX calculated in Update, calculated in Update in order to increase responsiveness, applied in FixedUpdate to keep physics interaction reliable

        return (Sprinting, currentMoveSpeedX);
    }

    private bool MovementY() // Responsible for the jumping, falling of the player 
    {
        groundTimer -= Time.deltaTime; // Decreasing the ground check timer

        bool groundTimerRunOut = groundTimer < 0; // Has ground check timer ran out?

        if (GetGrounded() && groundTimerRunOut) // If has landed and ground check timer ran out
        {
            groundTimer = GroundCheckTimer; // Reset ground check timer
            currentNumJumps = NumberOfExtraJumps + 1; // Restore all jumps
            fGroundedRemember = FGroundedRememberTime; // Restore remember that you are grounded timer
        }
        else if (groundTimerRunOut)
            groundTimer = GroundCheckTimer; // Reset ground check timer


        fJumpPressedRemember -= Time.deltaTime;
        bool JumpPressed = PlayerInput.JumpPressed;
        bool JumpReleased = PlayerInput.JumpReleased;

        if (JumpPressed) 
            fJumpPressedRemember = FJumpPressedRememberTime; //Reseting jump remember timer when player tries to jump
        if (JumpReleased) // When a player releases the Jump key
        {
            Vector2 velocity = RigidBody.velocity;
            if (velocity.y > 0) // If player is still going up
                RigidBody.velocity = new Vector2(velocity.x, velocity.y * FCutJumpHeight); // Decreases the y axis up velocity, so player stops moving up faster
        }

        bool jumped = false;
        if (fJumpPressedRemember > 0 && fGroundedRemember > 0) // When player still has jump remember timer and grounded remember timer
        {
            fJumpPressedRemember = 0; // Stop timer
            fGroundedRemember = 0; // Stop timer
            PlayerJump(); // Jump
            currentNumJumps--; // Decriment jumps available
            jumped = true;
        }
        else if (currentNumJumps > 0 && JumpPressed) // If in the air and try to jump
        {
            PlayerJump(); // Jump
            currentNumJumps--; // Decriment jumps available
            jumped = true;
        }

        //Going through the platforms
        float moveInputY = PlayerInput.MoveInputY;
        Physics2D.IgnoreLayerCollision(gameObject.layer, PlatformLayer, RigidBody.velocity.y > 0.0f || moveInputY < -0.5);
        
        return jumped;
    }
    
    private void PlayerJump()
    {
        RigidBody.velocity = new Vector2(RigidBody.velocity.x, JumpForce);
        //TODO: Call on Jump Event
        //PlayerManager.CallOnPlayerJumpedEvent();
    }

}
