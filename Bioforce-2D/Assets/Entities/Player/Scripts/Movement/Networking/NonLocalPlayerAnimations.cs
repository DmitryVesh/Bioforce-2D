using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonLocalPlayerAnimations : MonoBehaviour
{
    private GameObject PlayerModelObject { get; set; } = null;
    protected Animator Anim { get; set; } = null;
    private PlayerManager PlayerManager { get; set; } = null;
    private EntityWalking EntityWalking { get; set; } = null;

    protected bool IsLocalPlayer { get; set; } = false;

    //X axis animations
    public float RunSpeed { get; set; }
    protected float SpeedX { get; set; }
    [SerializeField] private bool FacingRight = true;

    //Y Axis animations


    //TODO: PlayerAnimation script which is put on nonLocal players, and then inhereted
    // and extended by LocalPlayer

    // 1 RunSpeed
    // 1 SpeedX
    // 1 Grounded
    // 0 Jumped

    protected virtual void Awake()
    {
        PlayerModelObject = transform.GetChild(0).gameObject;
        Anim = PlayerModelObject.GetComponent<Animator>();
        PlayerManager = GetComponent<PlayerManager>();
        PlayerManager.PlayerMovementStatsChanged += ChangedPlayerMovementStats; //Subscribe to the PlayerMovementStatsChanged event, so can change runSpeed

        if (!IsLocalPlayer)
            EntityWalking = GetComponent<EntityWalking>();
    }
    private void OnDestroy()
    {
        PlayerManager.PlayerMovementStatsChanged -= ChangedPlayerMovementStats;
    }
    protected virtual void FixedUpdate()
    {
        if (!IsLocalPlayer)
            SpeedX = PlayerManager.Velocity.x;

        XaxisAnimations();
        YaxisAnimations();
    }

    protected void ChangedPlayerMovementStats(float runSpeed)
    {
        RunSpeed = runSpeed;
    }

    private void XaxisAnimations()
    {
        float speedX = SpeedX;
        if (speedX < 0) //Moving left
        {
            speedX = -speedX; // Adjusting the horizontal speed to a positive value if moving to left
        }
        Anim.SetBool("Moving", speedX != 0);
        Anim.SetBool("Sprinting", speedX > RunSpeed);
    }
    protected virtual void YaxisAnimations()
    {
        Anim.SetBool("Grounded", EntityWalking.Grounded);
    }
    protected virtual void LateUpdate()
    {
        if ((SpeedX < 0 && FacingRight) || (SpeedX > 0 && !FacingRight))
        {
            FlipSprite();
        }
    }

    private void FlipSprite()
    {
        FacingRight = !FacingRight;
        PlayerModelObject.transform.Rotate(0, 180, 0);
    }
}
