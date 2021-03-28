using System;
using System.Collections;
using UnityEngine;

public class NonLocalPlayerAnimations : MonoBehaviour, IAnimations
{
    private GameObject PlayerModelObject { get; set; } = null;
    protected Animator Anim { get; set; } = null;
    protected IWalkingPlayer WalkingPlayer { get; set; } = null;

    private int OwnerClientID { get; set; } = -1;
    PlayerManager PlayerManager { get; set; } = null;


    //Fields for X axis animations
    [SerializeField] protected bool FacingRight = true;
    private float SpeedXNonLocal { get; set; }

    [SerializeField] private SpriteRenderer PlayerBodySprite;
    [SerializeField] private SpriteRenderer PlayerArmsSprite;

    [SerializeField] private GameObject SplatterPrefab;
    [SerializeField] private Sprite[] SplatterSprites;
    private SpriteRenderer SplatterSpriteRenderer { get; set; }
    private GameObject[] Splatters = new GameObject[NumMaxSplatters];
    private SpriteRenderer[] SplattersRenderers = new SpriteRenderer[NumMaxSplatters];
    const int NumMaxSplatters = 25;
    private int CurrentSplatterIndex = 0;

    static int CurrentSplatterOrderInLayerIndex = -32768;

    [SerializeField] private LinkedParticleSystemManager PlayerHitParticles;
    [SerializeField] private LinkedParticleSystemManager PlayerMuzzelFlashParticles;

    [SerializeField] private GameObject HitMarker;
    [SerializeField] private float DisplayHitMarkerTime = 0.25f;

    [SerializeField] private MinimapIcon MinimapPlayerIcon;

    [SerializeField] private ParticleSystem FootstepParticles;
    private ParticleSystem.EmissionModule FootstepEmission;
    private ParticleSystem.MinMaxCurve EmissionNormal;
    private ParticleSystem.MinMaxCurve EmissionSprint;
    [SerializeField] private float SprintingParticlesAdd = 15f;
    private ParticleSystem.MinMaxCurve EmissionEmpty;
    enum MovingState
    {
        notMoving,
        moving,
        sprinting
    }
    private MovingState CurrentMovingState { get; set; }

    [SerializeField] private ParticleSystem ImpactOnFlootParticles;
    private bool LastGrounded { get; set; } = true;

    //TODO: add Jumped sent animations so can play sound effect on non local players

    public void SetOwnerClientID(int iD)
    {
        OwnerClientID = iD;
    }
    internal void SetColor(Color playerColor)
    {
        PlayerBodySprite.color = playerColor;
        PlayerArmsSprite.color = playerColor;

        PlayerHitParticles.Initilise(playerColor, OwnerClientID);
        PlayerMuzzelFlashParticles.Initilise(playerColor, OwnerClientID);
        
        SplatterSpriteRenderer.color = playerColor;

        MinimapPlayerIcon.Color = playerColor;

        GenerateSplatters();
    }

    private void GenerateSplatters()
    {
        CurrentSplatterIndex = 0;
        int maxSplatterSprites = SplatterSprites.Length;
        Transform splatterHolder = new GameObject($"SplatterHolder: {OwnerClientID}").transform;
        splatterHolder.position = Vector3.zero;

        for (int splatterCount = 0; splatterCount < NumMaxSplatters; splatterCount++)
        {
            GameObject splatter = Instantiate(SplatterPrefab, splatterHolder);
            SpriteRenderer spriteRenderer = splatter.GetComponent<SpriteRenderer>();

            splatter.GetComponent<SpriteRenderer>().sprite = SplatterSprites[UnityEngine.Random.Range(0, maxSplatterSprites)];
            splatter.transform.localScale = new Vector2(UnityEngine.Random.Range(1f, 3f), UnityEngine.Random.Range(1f, 3f));

            splatter.SetActive(false);
            Splatters[splatterCount] = splatter;
            SplattersRenderers[splatterCount] = spriteRenderer;
        }
    }

    protected virtual void Awake()
    {
        PlayerModelObject = transform.GetChild(0).gameObject;
        Anim = PlayerModelObject.GetComponent<Animator>();
        
        WalkingPlayer = GetComponent<IWalkingPlayer>();

        SplatterSpriteRenderer = SplatterPrefab.GetComponent<SpriteRenderer>();

        FootstepEmission = FootstepParticles.emission;
        EmissionNormal = FootstepEmission.rateOverTime;
        EmissionSprint = new ParticleSystem.MinMaxCurve(EmissionNormal.constant + SprintingParticlesAdd);
        EmissionEmpty = new ParticleSystem.MinMaxCurve(0f);

        FootstepEmission.rateOverTime = EmissionEmpty;
    }
    private void Start()
    {
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];

        PlayerManager.OnPlayerDeath += DieAnimation;
        PlayerManager.OnPlayerRespawn += RespawnAnimation;
        PlayerManager.OnPlayerPaused += PlayerPaused;
        
        PlayerManager.OnPlayerTookDamage += LeaveSplatter;
        PlayerManager.OnPlayerTookDamage += PlayHitParticleEffect;
        PlayerManager.OnPlayerShot += PlayMuzzelFlashParticleEffect;

        HitMarker.SetActive(false);
        PlayerManager.OnPlayerTookDamage += ShowHitMarker;
    }

    private void ShowHitMarker(int damage, int currentHealth) =>
        StartCoroutine(DisplayHitMarker());
    private IEnumerator DisplayHitMarker()
    {
        HitMarker.SetActive(true);
        yield return new WaitForSeconds(DisplayHitMarkerTime);
        HitMarker.SetActive(false);
    }

    private void PlayMuzzelFlashParticleEffect(Vector2 position, Quaternion rotation) =>
        PlayerMuzzelFlashParticles.PlayAffect(position, rotation);
    private void PlayHitParticleEffect(int damage, int currentHealth) 
    {
        Transform tf = PlayerModelObject.transform;
        PlayerHitParticles.PlayAffect(tf.position, tf.rotation);
    }

    private void LeaveSplatter(int damage, int currentHealth)
    {
        GameObject splatter = Splatters[CurrentSplatterIndex];
        splatter.SetActive(true);
        splatter.transform.position = PlayerModelObject.transform.position;

        SpriteRenderer spriteRenderer = SplattersRenderers[CurrentSplatterIndex];
        spriteRenderer.sortingOrder = CurrentSplatterOrderInLayerIndex;

        CurrentSplatterOrderInLayerIndex++;
        CurrentSplatterIndex = (CurrentSplatterIndex + 1) % NumMaxSplatters;
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
            speedX = -speedX; // Adjusting the horizontal speed to a positive value if moving to left

        bool moving = speedX != 0;
        Anim.SetBool("Moving", moving);

        bool sprinting = speedX > WalkingPlayer.GetRunSpeed();
        Anim.SetBool("Sprinting", sprinting);
        
        HandleFootstepParticles(moving, sprinting);
    }

    private void HandleFootstepParticles(bool moving, bool sprinting)
    {
        MovingState newMovingState;
        bool grounded = WalkingPlayer.GetGrounded();
        if (sprinting && grounded)
            newMovingState = MovingState.sprinting;
        else if (moving && grounded)
            newMovingState = MovingState.moving;
        else
            newMovingState = MovingState.notMoving;

        if (newMovingState == CurrentMovingState)
            return;

        CurrentMovingState = newMovingState;
        switch (CurrentMovingState)
        {
            case MovingState.notMoving:
                FootstepEmission.rateOverTime = EmissionEmpty;
                break;
            case MovingState.moving:
                FootstepEmission.rateOverTime = EmissionNormal;
                break;
            case MovingState.sprinting:
                FootstepEmission.rateOverTime = EmissionSprint;
                break;
        }
    }

    public virtual void YAxisAnimations()
    {
        bool grounded = WalkingPlayer.GetGrounded();
        Anim.SetBool("Grounded", grounded);

        if (grounded == LastGrounded)
            return;

        LastGrounded = grounded;
        if (LastGrounded)
            ImpactOnFlootParticles.Play();
    }
    public virtual void DieAnimation(TypeOfDeath typeOfDeath)
    {
        Anim.SetTrigger("Die");
    }
    public virtual void RespawnAnimation()
    {
        Anim.SetTrigger("Respawn");
    }
    private void PlayerPaused(bool paused)
    {
        Anim.SetBool("Paused", paused);
    }

    protected virtual void LateUpdate()
    {
        if (CheckIfNeedToFlipSprite(WalkingPlayer.GetSpeedX()))
            FlipSprite();
    }
    protected bool CheckIfNeedToFlipSprite(float speedX)
    {
        if ((speedX < 0 && FacingRight) || (speedX > 0 && !FacingRight))
            return true;
        
        return false;
    }
    public virtual void FlipSprite()
    {
        FacingRight = !FacingRight;
        PlayerModelObject.transform.Rotate(0, 180, 0);
    }

    private void OnDestroy()
    {
        PlayerManager.OnPlayerDeath -= DieAnimation;
        PlayerManager.OnPlayerRespawn -= RespawnAnimation;
        PlayerManager.OnPlayerPaused -= PlayerPaused;
        PlayerManager.OnPlayerTookDamage -= PlayHitParticleEffect;
        PlayerManager.OnPlayerTookDamage -= LeaveSplatter;
        PlayerManager.OnPlayerShot -= PlayMuzzelFlashParticleEffect;
        PlayerManager.OnPlayerTookDamage -= ShowHitMarker;
    }

    
}
