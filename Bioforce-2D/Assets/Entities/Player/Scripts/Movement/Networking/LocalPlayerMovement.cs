using System;
using UnityEngine;

public class LocalPlayerMovement : NonLocalPlayerMovement
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

    

    // Variables used for PlayerAnimations
    // Grounded found in EntityWalking
    // SpeedX which is found in moving in X direction Data
    // runSpeed used to know when to switch from normal running animation to sprinting animation
    private bool Jumped { get; set; } = false; // Used to identify when player has jumped


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
        if (!CanMove) { return; } // Player shouldn't be able to move when dying, or initially respawing, so shouldn't move
        MovementX();
        MovementY();
    }
    protected virtual float GetMoveInputX()
    {
        return Input.GetAxisRaw("Horizontal"); // Get user input in x direction
    }
    protected virtual bool GetSprintInput()
    {
        return Input.GetButton("Sprint");
    }
    protected virtual bool GetJumpInputPressed()
    {
        return Input.GetButtonDown("Jump");
    }
    protected virtual bool GetJumpInputReleased()
    {
        return Input.GetButtonUp("Jump");
    }
    protected virtual float GetVerticalInput()
    {
        return Input.GetAxis("Vertical");
    }

    private void MovementX() // Calculates SpeedX that will be applied to player, in response to x axis inputs
    {
        float moveInputX = GetMoveInputX();

        if (GetSprintInput() && moveInputX != 0) // Player is moving and trying to sprint
        {
            if (currentStamina > 0) // Is stamina left
            {
                SpeedX = moveInputX * SprintSpeed; // Set moving speed to sprint speed
                currentStamina -= RateStaminaLoss * Time.deltaTime; // Lossing stamina
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
            float regenStamina = BoolToInt(currentStamina < MaxStamina) * (RateStaminaRegen * Time.deltaTime); // determines a regen value either: 0 if currentStamina >= MaxStamina, else regen rate
            currentStamina += regenStamina; // Regenerating stamina
        }
    }
    private void MovementXBranchless() // Calculates SpeedX that will be applied to player, in response to x axis inputs, using branchless (no if statements) technique, to improve execution speed, in fact it reduces exectution speed, after testing its slower...
    {
        float moveInputX = Input.GetAxisRaw("Horizontal"); // Get user input in x direction

        bool holdingSprintKey = Input.GetButton("Sprint"); // Is player holding down Sprint key
        bool isMoving = moveInputX != 0; // Is the player moving in any direction
        bool CanSprint = currentStamina > 0; // Does the player have any stamina left

        bool willSprint = holdingSprintKey && isMoving && CanSprint; // The player will sprint if all conditions met: sprint key being held, is moving left or right, has stamina left
        int willSprintMultiplier = BoolToInt(willSprint); // Turning the boolean value of True or false into 1 or 0, for calculating if sprint value should be sprintSpeed or 0

        float sprint = moveInputX * willSprintMultiplier * SprintSpeed; // Calculating if sprint value using boolean multiplier
        currentStamina -= RateStaminaLoss * willSprintMultiplier * Time.deltaTime; // Decreasing stamina, only if the player will be sprinting

        bool noSprint = !holdingSprintKey || !isMoving || !CanSprint; // Checking if player is not sprinting, conditions that must be met: not holding "sprint" key, or not moving left or right, or can't spirnt
        bool canRegen = (currentStamina < MaxStamina) && !holdingSprintKey; // Checking if can regen stamina, 

        int noSprintMultiplier = BoolToInt(noSprint); // Turning the boolean value of True or false into 1 or 0, for calculating if stamina regen value should be RateStaminaRegen * Time.deltaTime or 0 
        int canRegenMultiplier = BoolToInt(canRegen); // Turning the boolean value of True or false into 1 or 0, for calculating if stamina regen value should be RateStaminaRegen * Time.deltaTime or 0

        float runOrIdle = moveInputX * RunSpeed; // Calculating run or idle speed, will always be either 0, moving left or right
        currentStamina += RateStaminaRegen * noSprintMultiplier * canRegenMultiplier * Time.deltaTime; // Regenerating stamina, with 0 values if doesn't satisfy branchless boolean multiplier, or positive RateStaminaRegen * Time.deltaTime

        bool movingLeft = moveInputX < 0; // Boolean statements that check if player is moving left

        SpeedX = ReturnLarger(sprint, runOrIdle) * BoolToInt(!movingLeft) + (ReturnSmaller(sprint, runOrIdle) * BoolToInt(movingLeft)); // Setting the speed variable that is used in fixed update with the lowest speed, or highest speed
    }
    private void MovementXHybridBranchless()
    {
        float moveInputX = Input.GetAxisRaw("Horizontal"); // Get user input in x direction

        bool holdingSprintKey = Input.GetButton("Sprint"); // Is player holding down Sprint key
        bool isMoving = moveInputX != 0; // Is the player moving in any direction
        bool CanSprint = currentStamina > 0; // Does the player have any stamina left

        float sprint = 0;
        if (holdingSprintKey && isMoving && CanSprint) // The player will sprint if all conditions met: sprint key being held, is moving left or right, has stamina left
        {
            sprint = moveInputX * SprintSpeed; // Calculating if sprint value using boolean multiplier
            currentStamina -= RateStaminaLoss * Time.deltaTime; // Decreasing stamina, only if the player will be sprinting
        }

        bool noSprint = !holdingSprintKey || !isMoving || !CanSprint; // Checking if player is not sprinting, conditions that must be met: not holding "sprint" key, or not moving left or right, or can't spirnt
        bool canRegen = (currentStamina < MaxStamina) && !holdingSprintKey; // Checking if can regen stamina, 

        float runOrIdle = moveInputX * RunSpeed; // Calculating run or idle speed, will always be either 0, moving left or right
        if (noSprint && canRegen)
        {
            currentStamina += RateStaminaRegen * Time.deltaTime; // Regenerating stamina, with 0 values if doesn't satisfy branchless boolean multiplier, or positive RateStaminaRegen * Time.deltaTime
        }

        if (moveInputX < 0) // Boolean statements that check if player is moving left
        {
            SpeedX = ReturnSmaller(sprint, runOrIdle);
        }
        else
        {
            SpeedX = ReturnLarger(sprint, runOrIdle);
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
        {
            groundTimer = GroundCheckTimer; // Reset ground check timer
        }


        fJumpPressedRemember -= Time.deltaTime;
        bool JumpPressed = GetJumpInputPressed();

        if (JumpPressed) { fJumpPressedRemember = FJumpPressedRememberTime; } //Reseting jump remember timer when player tries to jump
        if (GetJumpInputReleased()) // When a player releases the Jump key
        {
            Vector2 velocity = rb.velocity;
            if (velocity.y > 0) // If player is still going up
            { 
                rb.velocity = new Vector2(velocity.x, velocity.y * FCutJumpHeight); // Decreases the y axis up velocity, so player stops moving up faster
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

        Physics2D.IgnoreLayerCollision(gameObject.layer, PlatformLayer, rb.velocity.y > 0.0f || GetVerticalInput() < -0.5);
    }
    private void PlayerJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, JumpForce);
        Jumped = true;
    }
    

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        Vector2 velocity = new Vector2(SpeedX, rb.velocity.y);
        rb.velocity = velocity; // Applying movement in x direction, SpeedX calculated in Update, calculated in Update in order to increase responsiveness, applied in FixedUpdate to keep physics interaction reliable
        
        SendMovesToServer(velocity);
    }

    private void SendMovesToServer(Vector3 velocity)
    {
        ClientSend.PlayerMovement(ModelObject.transform.rotation, ModelObject.transform.position, velocity);
    }


    //TODO: make a seperate class for testing 
    //TODO: make a class for branchless methods
    void MethodTimeTesting(Action method, int iterationsPower10)
    {
        //Used for testing execution time of methods
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        for (int count = 0; count < Math.Pow(10, iterationsPower10); count++)
        {
            method.Invoke();
        }
        watch.Stop();
        Debug.Log($"Execution time of {method.Method.Name}: {watch.ElapsedMilliseconds}");
    }
    public int BoolToInt(bool value) // Converts boolean values True/False to 1/0, used in branchless programming
    {
        return value ? 1 : 0;
    }
    public float ReturnLarger(float a, float b)
    {
        if (a > b) { return a; }
        return b;
        //return (a * BoolToInt(a >= b)) + (b * BoolToInt(a < b));
    }
    public float ReturnSmaller(float a, float b)
    {
        if (a > b) { return a; }
        return b;
        //return (a * BoolToInt(a < b)) + (b * BoolToInt(a >= b));
    }
    public float ReturnMod(float value, bool condition)
    {
        return (value * -1 * BoolToInt(condition)) + (value * 1 * BoolToInt(!condition));
    }
}
