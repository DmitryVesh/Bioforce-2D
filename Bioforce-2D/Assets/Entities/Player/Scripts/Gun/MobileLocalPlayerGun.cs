using UnityEngine;

public class MobileLocalPlayerGun : LocalPlayerGun
{
    private bool PlayerTriedToShoot { get; set; } = false;
    private Joystick Joystick { get; set; }
    [SerializeField] private float TimeBetweenShots = 0.20f;
    private float TimeNextShot { get; set; }
    private bool AlreadyTouchedJoystick { get; set; }
    private float TimeCanFireAfterFirstTouch { get; set; }
    
    public float LastX { get; set; }
    public float LastY { get; set; }

    protected override void Awake()
    {
        base.Awake();

        Joystick = ShootingJoystick.Instance.GetComponent<Joystick>();
        ShootingJoystick.Instance.SetActive(true);
    }
    protected override void Start()
    {
        base.Start();
    }
    protected override void Update()
    {
        bool holdingJoystick = Joystick.Horizontal != 0 || Joystick.Vertical != 0;
        if (holdingJoystick && !AlreadyTouchedJoystick)
        {
            //Wait for like 0.05s before shooting
            TimeCanFireAfterFirstTouch = Time.time;// + 0.05f;
            AlreadyTouchedJoystick = true;
            return;
        }
        else if (!holdingJoystick)
            AlreadyTouchedJoystick = false;

        if (Time.time >= TimeCanFireAfterFirstTouch && holdingJoystick && Time.time >= TimeNextShot)
        {
            TimeNextShot = Time.time + TimeBetweenShots;
            SetPlayerTriedToShootTrue();
        }

        base.Update();
    }
    protected override void AimWherePointing()
    {
        float x = Joystick.Horizontal;
        float y = Joystick.Vertical;

        if (x != 0 && y != 0)
        {
            LastX = x;
            LastY = y;
        }

        Aim(LastX, LastY);
        LastAimVector = new Vector2(LastX, LastY);
    }

    protected override bool IsPlayerTryingToShoot()
    {
        if (PlayerTriedToShoot)
        {
            PlayerTriedToShoot = false;
            return true;
        }
        return false;
    }
    private void SetPlayerTriedToShootTrue()
    {
        PlayerTriedToShoot = true;
    }

    
}
