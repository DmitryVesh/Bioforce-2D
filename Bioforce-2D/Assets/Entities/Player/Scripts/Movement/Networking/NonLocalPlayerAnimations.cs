using System;
using System.Collections;
using UnityEngine;

public class NonLocalPlayerAnimations : MonoBehaviour, IAnimations
{
    private GameObject PlayerModelObject { get; set; } = null;
    protected Animator Anim { get; set; } = null;
    protected IWalkingPlayer WalkingPlayer { get; set; } = null;

    private byte OwnerClientID { get; set; } = 255;
    PlayerManager PlayerManager { get; set; } = null;


    //Fields for X axis animations
    [SerializeField] protected bool FacingRight = true;
    private float SpeedXNonLocal { get; set; }

    [SerializeField] private SpriteRenderer PlayerBodySprite;
    [SerializeField] private SpriteRenderer PlayerArmsSprite;

    [SerializeField] private GameObject SplatterPrefab;
    [SerializeField] private Sprite[] SplatterSprites;

    private Transform SplatterHolder; //Set when initing splatters
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

    [SerializeField] [Range(0.1f, 1)] private float TimeBeforeStoppingFlashingCutMultiplier = 0.875f;
    private Coroutine FlashCoroutine;
    //TODO: add Jumped sent animations so can play sound effect on non local players

    public void SetOwnerClientID(byte iD)
    {
        OwnerClientID = iD;
    }
    internal void SetColor(Color playerColor, Transform gunTransform)
    {
        SetPlayerModelColor(playerColor);

        PlayerHitParticles.Initilise(playerColor, PlayerModelObject.transform);
        PlayerMuzzelFlashParticles.Initilise(playerColor, gunTransform);

        SplatterSpriteRenderer.color = playerColor;

        MinimapPlayerIcon.Color = playerColor;

        GenerateSplatters();
    }
    private void SetPlayerModelColor(Color playerColor)
    {
        PlayerBodySprite.color = playerColor;
        PlayerArmsSprite.color = playerColor;
    }

    private void GenerateSplatters()
    {
        CurrentSplatterIndex = 0;
        int maxSplatterSprites = SplatterSprites.Length;
        SplatterHolder = new GameObject($"SplatterHolder: {OwnerClientID}").transform;
        SplatterHolder.position = Vector3.zero;

        for (int splatterCount = 0; splatterCount < NumMaxSplatters; splatterCount++)
        {
            GameObject splatter = Instantiate(SplatterPrefab, SplatterHolder);
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

        PlayerManager.OnPlayerInvincibility += InvincibilityAnimation;
        PlayerManager.OnPlayerRespawn += PlayRespawnInvincibilityAnimation;
    }
    private void OnDestroy()
    {
        CleanUpSplatters();

        PlayerManager.OnPlayerDeath -= DieAnimation;
        PlayerManager.OnPlayerRespawn -= RespawnAnimation;
        PlayerManager.OnPlayerPaused -= PlayerPaused;
        PlayerManager.OnPlayerTookDamage -= PlayHitParticleEffect;
        PlayerManager.OnPlayerTookDamage -= LeaveSplatter;
        PlayerManager.OnPlayerShot -= PlayMuzzelFlashParticleEffect;
        PlayerManager.OnPlayerTookDamage -= ShowHitMarker;
        PlayerManager.OnPlayerInvincibility -= InvincibilityAnimation;
        PlayerManager.OnPlayerRespawn -= PlayRespawnInvincibilityAnimation;
    }

    private void CleanUpSplatters()
    {
        if (SplatterHolder && SplatterHolder.gameObject)
            Destroy(SplatterHolder.gameObject);

        foreach (GameObject splatterObj in Splatters)
        {
            if (splatterObj == null)
                continue;
            Destroy(splatterObj.gameObject);
        }
    }

    private void PlayRespawnInvincibilityAnimation()
    {
        InvincibilityAnimation(PlayerManager.RespawnTime + PlayerManager.InvincibilityTimeAfterRespawning);
    }
    private void InvincibilityAnimation(float invincibilityTime)
    {
        if (!(FlashCoroutine is null))
            StopCoroutine(FlashCoroutine);

        FlashCoroutine = StartCoroutine(FlashPlayer(invincibilityTime));
    }
    private IEnumerator FlashPlayer(float flashFor)
    {
        Anim.SetBool("Flash", true);
        yield return new WaitForSeconds(flashFor * TimeBeforeStoppingFlashingCutMultiplier);
        Anim.SetBool("Flash", false);
    }

    private void ShowHitMarker(int currentHealth) =>
        StartCoroutine(DisplayHitMarker());
    private IEnumerator DisplayHitMarker()
    {
        HitMarker.SetActive(true);
        yield return new WaitForSeconds(DisplayHitMarkerTime);
        HitMarker.SetActive(false);
    }

    private void PlayMuzzelFlashParticleEffect(Vector2 _, Quaternion __) =>
        PlayerMuzzelFlashParticles.PlayAffect();
    private void PlayHitParticleEffect(int currentHealth) =>
        PlayerHitParticles.PlayAffect();

    private void LeaveSplatter(int currentHealth)
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

    

    
}
