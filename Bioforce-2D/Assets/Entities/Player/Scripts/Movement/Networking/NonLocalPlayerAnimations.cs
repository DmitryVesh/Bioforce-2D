using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonLocalPlayerAnimations : MonoBehaviour, IAnimations
{
    private GameObject PlayerModelObject { get; set; } = null;
    protected Animator Anim { get; set; } = null;
    protected IWalkingPlayer WalkingPlayer { get; set; } = null;

    private int OwnerClientID { get; set; } = -1;
    PlayerManager PlayerManager { get; set; } = null;


    //Fields for X axis animations
    [SerializeField] private bool FacingRight = true;
    private float SpeedXNonLocal { get; set; }

    //TODO: add Jumped sent animations so can play sound effect

    public void SetOwnerClientID(int iD)
    {
        OwnerClientID = iD;
    }

    protected virtual void Awake()
    {
        PlayerModelObject = transform.GetChild(0).gameObject;
        Anim = PlayerModelObject.GetComponent<Animator>();
        
        WalkingPlayer = GetComponent<IWalkingPlayer>();
    }
    private void Start()
    {
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];

        PlayerManager.OnPlayerDeath += DieAnimation;
        PlayerManager.OnPlayerRespawn += RespawnAnimation;
    }
    protected virtual void FixedUpdate()
    {
        SpeedXNonLocal = WalkingPlayer.GetSpeedX();
        XAxisAnimations(SpeedXNonLocal);
        YAxisAnimations();
    }

    
    public void XAxisAnimations(float speedX)
    {
        if (speedX < 0) //Moving left
        {
            speedX = -speedX; // Adjusting the horizontal speed to a positive value if moving to left
        }
        Anim.SetBool("Moving", speedX != 0);
        Anim.SetBool("Sprinting", speedX > WalkingPlayer.GetRunSpeed());
    }
    public virtual void YAxisAnimations()
    {
        Anim.SetBool("Grounded", WalkingPlayer.GetGrounded());
    }
    public virtual void DieAnimation()
    {
        Anim.SetTrigger("Die");
    }
    public virtual void RespawnAnimation()
    {
        Anim.SetTrigger("Respawn");
    }


    protected virtual void LateUpdate()
    {
        if (CheckIfNeedToFlipSprite(WalkingPlayer.GetSpeedX()))
            FlipSprite();
    }
    protected bool CheckIfNeedToFlipSprite(float speedX)
    {
        if ((speedX < 0 && FacingRight) || (speedX > 0 && !FacingRight))
        {
            return true;
        }
        return false;
    }
    public void FlipSprite()
    {
        FacingRight = !FacingRight;
        PlayerModelObject.transform.Rotate(0, 180, 0);
    }

    private void OnDestroy()
    {
        PlayerManager.OnPlayerDeath -= DieAnimation;
        PlayerManager.OnPlayerRespawn -= RespawnAnimation;
    }

}
