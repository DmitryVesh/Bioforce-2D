using System.Collections;
using UnityEngine;

public class LocalPlayerMovement : NonLocalPlayerMovement, IWalkingLocalPlayer
{
    // Stamina Data
    [SerializeField] private float MaxStamina = 5; // Used as time limit for how much a player can sprint
    [SerializeField] private float RateStaminaLoss = 1; // Used as value for the rate of stamina lost when sprinting
    [SerializeField] private float RateStaminaRegen = 0.5f; // Used as value for the rate of stamina regenerated when not sprinting
    private float currentStamina; // Used as the stamina left to the player


    // Moving in Y direction Data
    [SerializeField] private string PlatformLayerName = "Platform"; // LayerMask used to determine what is a platform, so can go through that layer
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

    private Vector2 LastPosition { get; set; }
    private PlayerMovingState LastMoveState { get; set; }
    private bool LastMovingLeft { get; set; }

    // Variables used for PlayerAnimations
    // Grounded found in EntityWalking
    // SpeedX which is found in moving in X direction Data
    // runSpeed used to know when to switch from normal running animation to sprinting animation
    private bool Jumped { get; set; } = false; // Used to identify when player has jumped, for LocalPlayerAnimations
    

    public float GetMaxStamina() =>
        MaxStamina;
    public float GetCurrentStamina() =>
        currentStamina;

    public bool HasJumped()
    {
        bool result = Jumped;
        Jumped = false;
        return result;
    }

    protected override void Awake()
    {
        base.Awake();

        CanMove = true;
        currentStamina = MaxStamina;

        ClientSend.PlayerMovementStats(RunSpeed, SprintSpeed);
        PlatformLayer = LayerMask.NameToLayer(PlatformLayerName);
    }
    
    void Update() // Used for getting user input, and storing it later to be used in FixedUpdate
    {
        if (!CanMove) 
        {        
            return; // Player shouldn't be able to move when dying, or initially respawing, so shouldn't move
        } 
        MovementX();
        MovementY();
    }
    protected virtual float GetMoveInputX() =>
        Input.GetAxisRaw("Horizontal"); // Get user input in x direction
    protected virtual bool GetSprintInput() =>
        Input.GetButton("Sprint");

    protected virtual bool GetJumpInputPressed() =>
        Input.GetButtonDown("Jump");
    protected virtual bool GetJumpInputReleased() =>
        Input.GetButtonUp("Jump");
    protected virtual float GetVerticalInput() =>
        Input.GetAxis("Vertical");

    private void MovementX() // Calculates SpeedX that will be applied to player, in response to x axis inputs
    {
        float moveInputX = GetMoveInputX();
        bool moving = moveInputX != 0;
        bool sprinting = false;

        if (GetSprintInput() && moving) // Player is moving and trying to sprint
        {
            if (currentStamina > 0) // Is stamina left
            {
                SpeedX = moveInputX * SprintSpeed; // Set moving speed to sprint speed
                currentStamina -= RateStaminaLoss * Time.deltaTime; // Lossing stamina
                sprinting = true;
            }
            else
            {
                //TODO: Give a player cooldown after fully exhausting the stamina bar
                SpeedX = moveInputX * RunSpeed; // Set moving speed to normal running speed
            }
        }
        else // Player is either not moving, moving, or is holding shift without moving  
        {
            SpeedX = moveInputX * RunSpeed; // Set moving speed to normal running speed
            float regenStamina = ((currentStamina < MaxStamina) ? 1 : 0) * (RateStaminaRegen * Time.deltaTime); // determines a regen value either: 0 if currentStamina >= MaxStamina, else regen rate
            currentStamina += regenStamina; // Regenerating stamina
        }

        //States 
        bool goingLeft;
        if (!moving)
            goingLeft = LastMovingLeft;
        else
            goingLeft = moveInputX < 0;

        if (sprinting) 
        {
            if (goingLeft)
                CurrentMovingState = PlayerMovingState.sprintingLeft;
            else
                CurrentMovingState = PlayerMovingState.sprintingRight;
        }
        else if (moving)
        {
            if (goingLeft)
                CurrentMovingState = PlayerMovingState.runningLeft;
            else
                CurrentMovingState = PlayerMovingState.runningRight;
        }
        else
        {
            if (goingLeft)
                CurrentMovingState = PlayerMovingState.idleLeft;
            else
                CurrentMovingState = PlayerMovingState.idleRight;
        }
    }
    
    private void MovementY() // Responsible for the jumping, falling of the player 
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
        bool JumpPressed = GetJumpInputPressed();

        if (JumpPressed) { fJumpPressedRemember = FJumpPressedRememberTime; } //Reseting jump remember timer when player tries to jump
        if (GetJumpInputReleased()) // When a player releases the Jump key
        {
            Vector2 velocity = RigidBody.velocity;
            if (velocity.y > 0) // If player is still going up
            { 
                RigidBody.velocity = new Vector2(velocity.x, velocity.y * FCutJumpHeight); // Decreases the y axis up velocity, so player stops moving up faster
            }
        }


        if (fJumpPressedRemember > 0 && fGroundedRemember > 0) // When player still has jump remember timer and grounded remember timer
        {
            fJumpPressedRemember = 0; // Stop timer
            fGroundedRemember = 0; // Stop timer
            PlayerJump(); // Jump
            currentNumJumps--; // Decriment jumps available
        }
        else if (currentNumJumps > 0 && JumpPressed) // If in the air and try to jump
        {
            PlayerJump(); // Jump
            currentNumJumps--; // Decriment jumps available
        }

        Physics2D.IgnoreLayerCollision(gameObject.layer, PlatformLayer, RigidBody.velocity.y > 0.0f || GetVerticalInput() < -0.5);
    }
    private void PlayerJump()
    {
        RigidBody.velocity = new Vector2(RigidBody.velocity.x, JumpForce);
        Jumped = true;
        PlayerManager.CallOnPlayerJumpedEvent();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        Vector2 velocity = new Vector2(SpeedX, RigidBody.velocity.y);
        if (CanMove)
            RigidBody.velocity = velocity; // Applying movement in x direction, SpeedX calculated in Update, calculated in Update in order to increase responsiveness, applied in FixedUpdate to keep physics interaction reliable

        SendMovesToServer();
    }

    protected override IEnumerator UnFreezeMotionAndHitBoxAfterRespawnAnimation()
    {
        SetRespawnPointLocation();
        yield return base.UnFreezeMotionAndHitBoxAfterRespawnAnimation();
    }
    protected override void UnFreezeMotion()
    {
        CanMove = true;
        RigidBody.bodyType = RigidbodyType2D.Dynamic;
    }
    private void SetRespawnPointLocation()
    {
        ModelObject.transform.position = RespawnPoint.GetRandomSpawnPoint(ModelObject.transform.position);
        PlayerManager.RespawnPosition = ModelObject.transform.position;
        RigidBody.velocity.Set(0, 0);
    }
    protected override void PlayerCanMoveAndCanBeHit()
    {
        currentStamina = MaxStamina;
        base.PlayerCanMoveAndCanBeHit();
    }

    private void SendMovesToServer()
    {
        
        Vector2 playerPosition = (Vector2)ModelObject.transform.position;
        if (LastPosition != playerPosition) 
        {
            Client.Instance.FlagWorldPositionToBeSent(playerPosition);
            LastPosition = playerPosition;
        }

        if (LastMoveState != CurrentMovingState)
        {
            Client.Instance.FlagMoveStateToBeSent(CurrentMovingState);
            LastMoveState = CurrentMovingState;
        }

        //ClientSend.PlayerMovement(ModelObject.transform.position, CurrentMovingState);
    }

}
