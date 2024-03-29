﻿using System.Collections.Generic;
using UnityEngine;

public class NonLocalPlayerGun : MonoBehaviour, IGun
{
    [SerializeField] protected GameObject BulletPrefab;
    private Transform BulletHolder; //Instantiate when initing  
    private List<Bullet> BulletList = new List<Bullet>();

    private byte OwnerClientID = 255;
    protected bool CanShootAndAim = true;
    protected PlayerManager PlayerManager { get; set; } = null;
    
    [SerializeField] private MuzzelFlash MuzzelFlash; //Set in inspector

    [SerializeField] protected Transform ArmsTransform;
    private Vector2 ArmPosition { get; set; }
    private Quaternion ArmRotation{ get; set; }

    public Color PlayerColor { get; set; }
    public Collider2D OwnCollider { get; private set; }

    public virtual void ShootBullet(Vector2 position, Quaternion rotation) //Has to be public to satisfy interface
    {
        Bullet bullet;
        bullet = GetBullet();

        if (bullet == null)
        {
            bullet = AddBullet();
            bullet.SetBulletColor(PlayerColor);
        }

        bullet.Shoot(position, rotation);
        Physics2D.IgnoreCollision(bullet.Hitbox, OwnCollider, true);

        MuzzelFlash.PlayFlash();
    }

    #region Interface Methods
    public void SetOwnerClientID(byte iD)
    {
        OwnerClientID = iD;

        BulletHolder = new GameObject($"BulletHolder: {OwnerClientID}").transform;
        BulletHolder.position = Vector3.zero;
    }
    public void SetOwnerCollider(Collider2D ownCollider) =>
        OwnCollider = ownCollider;
    private void SetArmRotation(Quaternion rotation) => //Need to call in late update so anims don't override the position x...
        ArmRotation = rotation;
    private void SetArmPosition(Vector2 position) => //Need to call in late update so anims don't override the position x...
        ArmPosition = position;
    public void Disable(TypeOfDeath typeOfDeath) =>
        CanShootAndAim = false;
    public void Enable() =>
        Invoke("SetCanShootTrue", PlayerManager.RespawnTime);
    private void SetCanShootTrue() => //Needed for Enable
        CanShootAndAim = true;
    public void SetColor(Color playerColor) =>
        PlayerColor = playerColor;
    public Transform GetTransform() =>
        ArmsTransform.transform;
    #endregion

    protected virtual void Start()
    {
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];

        PlayerManager.OnPlayerDeath += Disable;
        PlayerManager.OnPlayerRespawn += Enable;
        PlayerManager.OnPlayerShot += ShootBullet;
        PlayerManager.OnPlayerArmPosition += SetArmPosition;
        PlayerManager.OnPlayerArmRotation += SetArmRotation;

        SetToPlayerColor(MuzzelFlash.gameObject);
    }
    protected virtual void LateUpdate()
    {
        ArmsTransform.localPosition = ArmPosition;
        ArmsTransform.localRotation = ArmRotation;
    }

    private Bullet AddBullet()
    {
        GameObject bulletGameObject = Instantiate(BulletPrefab, Vector3.zero, Quaternion.identity, BulletHolder);
        SetToPlayerColor(bulletGameObject);
        Bullet bulletScript = bulletGameObject.GetComponent<Bullet>();
        bulletScript.SetOwner(OwnerClientID);
        
        BulletList.Add(bulletScript);
        return bulletScript;
    }

    private void SetToPlayerColor(GameObject bulletGameObject) =>
        bulletGameObject.GetComponent<SpriteRenderer>().color = PlayerColor;

    private Bullet GetBullet()
    {
        Bullet bulletToShoot = null;
        foreach (Bullet bullet in BulletList)
        {
            if (bullet.IsAvailable())
            {
                bulletToShoot = bullet;
                break;
            }
        }
        return bulletToShoot;
    }

    private void CleanUpBullets()
    {
        if (BulletHolder && BulletHolder.gameObject)
            Destroy(BulletHolder.gameObject);

        foreach (Bullet bullet in BulletList)
        {
            if (bullet == null)
                continue;
            Destroy(bullet.gameObject);
        }
        BulletList.Clear();
    }

    protected virtual void OnDestroy()
    {
        CleanUpBullets();        

        PlayerManager.OnPlayerDeath -= Disable;
        PlayerManager.OnPlayerRespawn -= Enable;
        PlayerManager.OnPlayerShot -= ShootBullet;
        PlayerManager.OnPlayerArmPosition -= SetArmPosition;
        PlayerManager.OnPlayerArmRotation -= SetArmRotation;
    }

}
