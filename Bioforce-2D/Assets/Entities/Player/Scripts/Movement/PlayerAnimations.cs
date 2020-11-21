using UnityEngine;
using System;

public class PlayerAnimations : MonoBehaviour
{
    private Animator anim = null;
    //Need to find out how to just make a Pointer to the data of SpeedX, isGrounded in PlayerMovement 
    //Shouldn't use Pointers because it means the data has to be stored permentantly, therefore longer garbage collection...
    //Probably less of an issue if just cache the PlayerMovement 
    //private float* SpeedX;
    //private bool* isGrounded;
    private PlayerMovement movementScript = null;
    private Transform tf = null;
    private float SpeedX;
    [SerializeField] private bool facingRight = true;


    public void JumpAnimation()
    {
        anim.SetTrigger("Jumped");
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        movementScript = GetComponent<PlayerMovement>();
        tf = GetComponent<Transform>();
    }
    private void Start()
    {
        //Events_PlayerAnimations.instance.onMovingChanged += UpdateAnimControllerMoving;
        //Events_PlayerAnimations.instance.onGroundedChanged += UpdateAnimControllerGrounded;
    }

    private void FixedUpdate()
    {
        float runSpeed;
        float SpeedX;
        bool grounded;
        bool jumped;
        (runSpeed, SpeedX, grounded, jumped) = movementScript.GetAnimationData();

        XaxisAnimations(runSpeed, SpeedX);
        YaxisAnimations(grounded, jumped);

        
    }
    private void XaxisAnimations(float runSpeed, float SpeedX)
    {
        this.SpeedX = SpeedX; // Set, used in LateUpdate for flipping the sprite 
        if (SpeedX < 0) //Moving left
        {
            SpeedX = -SpeedX; // Adjusting the horizontal speed to a positive value if moving to left
        }
        
        bool Moving = false, Sprinting = false;

        if (SpeedX > runSpeed)
        {
            Moving = true;
            Sprinting = true;
        }
        else if (SpeedX != 0)
        {
            Moving = true;
        }

        anim.SetBool("Moving", Moving);
        anim.SetBool("Sprinting", Sprinting);
        
    }
    private void YaxisAnimations(bool Grounded, bool Jumped)
    {
        
        if (Jumped)
        {
            anim.SetBool("Grounded", false);
            anim.SetTrigger("Jumped");
        }
        else
        {
            anim.SetBool("Grounded", Grounded);
        }
    }

    private void LateUpdate()
    {
        if ((SpeedX < 0 && facingRight) || (SpeedX > 0 && !facingRight))
        {
            FlipSprite();
        }
    }

    private void FlipSprite()
    {
        facingRight = !facingRight;
        tf.Rotate(0, 180, 0);
    }

    

    
}
