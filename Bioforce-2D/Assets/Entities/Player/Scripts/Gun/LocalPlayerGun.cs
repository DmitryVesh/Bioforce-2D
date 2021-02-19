using UnityEngine;

public class LocalPlayerGun : NonLocalPlayerGun
{
    [SerializeField] private Transform FirePointTransform; //Set in inspector    

    private Camera MainCamera { get; set; }

    private bool FacingRight { get; set; } = true;
    private float RotateExtra { get; set; }
    private float TurnedPositionOffset { get; set; } = 0.137f;

    public void SetLookDir(bool facingRight)
    {
        FacingRight = facingRight;
        if (FacingRight)
            RotateExtra = 0;
        else
        {
            RotateExtra = 180;
        }
    }
    protected override void Awake()
    {
        base.Awake();
        MainCamera = Camera.main;
        Physics2D.IgnoreLayerCollision(11, 16, true); //Ignore layer collision between LocalPlayer and Bullet LocalPlayer
    }
    protected override void Start()
    {
        base.Start();
        GameManager.Instance.OnPauseEvent += SetCanShootAndAim;
    }

    protected virtual bool IsPlayerTryingToShoot()
    {
        //TODO: add automatic shooting GetButton instead of Down
        return Input.GetButtonDown("Fire1");
    }
    protected virtual void Update()
    {
        if (IsPlayerTryingToShoot() && CanShootAndAim)
            ShootBullet();
    }

    protected override void LateUpdate()
    {
        if (CanShootAndAim)
            AimWherePointing();
        ClientSend.ArmPositionAndRotation(ArmsTransform.localPosition, ArmsTransform.localRotation);
    }

    protected virtual void AimWherePointing()
    {
        Vector3 difference = MainCamera.ScreenToWorldPoint(Input.mousePosition) - ArmsTransform.position;
        difference.Normalize();

        Aim(difference.x, difference.y);
    }

    protected void Aim(float x, float y)
    {
        float rotationZ = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        ArmsTransform.rotation = Quaternion.Euler(0, 0, rotationZ);

        bool aimingRight = rotationZ > -90 && rotationZ < 90;
        bool aimingLeft = rotationZ < -90 || rotationZ > 90;


        if ((aimingRight && FacingRight) || (aimingLeft && !FacingRight))
            ArmsTransform.localPosition = new Vector2(-0.24f, ArmsTransform.localPosition.y);
        else
            ArmsTransform.localPosition = new Vector2(TurnedPositionOffset, ArmsTransform.localPosition.y);


        if (rotationZ < -90 || rotationZ > 90)
        {
            if (transform.eulerAngles.y == 0)
                ArmsTransform.localRotation = Quaternion.Euler(180, 0 + RotateExtra, -rotationZ); //Flip gun right
            else if (transform.eulerAngles.y == 180)
                ArmsTransform.localRotation = Quaternion.Euler(180, 180 + RotateExtra, -rotationZ); //Flip gun left
        }
    }

    private void SetCanShootAndAim(bool paused)
    {
        CanShootAndAim = !paused;
    }
    private void ShootBullet()
    {
        Crosshair.Instance.ShotBullet();
        PlayerManager.CallOnBulletShotEvent(FirePointTransform.position, FirePointTransform.rotation);
        //base.ShootBullet(FirePointTransform.position, FirePointTransform.rotation);
        ClientSend.ShotBullet(FirePointTransform.position, FirePointTransform.rotation);
        
    }

    
}
