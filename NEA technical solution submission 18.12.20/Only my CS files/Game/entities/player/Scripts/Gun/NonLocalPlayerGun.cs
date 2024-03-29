﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NonLocalPlayerGun : MonoBehaviour, IGun
{
    [SerializeField] protected GameObject BulletPrefab;
    protected List<Bullet> BulletList;

    private int OwnerClientID = -1;
    protected bool CanShoot = true;
    protected PlayerManager PlayerManager { get; set; } = null;

    [SerializeField] private Color PlayerColor; //Set in inspector, colors the muzzel flash and bulletPrefab
    [SerializeField] private MuzzelFlash MuzzelFlash; //Set in inspector

    public virtual void ShootBullet(Vector2 position, Quaternion rotation)
    {
        Bullet bullet;
        bullet = GetBullet();

        if (bullet == null)
        {
            bullet = AddBullet();
            bullet.SetBulletColor(PlayerColor);
        }

        bullet.Shoot(position, rotation);

        MuzzelFlash.PlayFlash();
    }
    public void SetOwnerClientID(int iD)
    {
        OwnerClientID = iD;
    }
    
    public void Disable(TypeOfDeath typeOfDeath) =>
        CanShoot = false;

    public void Enable() =>
        Invoke("SetCanShootTrue", PlayerManager.RespawnTime);

    private void SetCanShootTrue() =>
        CanShoot = true;

    private void Awake()
    {
        BulletList = new List<Bullet>();
    }
    protected virtual void Start()
    {
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];
        PlayerManager.OnPlayerDeath += Disable;
        PlayerManager.OnPlayerRespawn += Enable;
        PlayerManager.OnPlayerShot += ShootBullet;
        SetToPlayerColor(MuzzelFlash.gameObject);
    }

    private Bullet AddBullet()
    {
        GameObject bulletGameObject = Instantiate(BulletPrefab, Vector3.zero, Quaternion.identity);
        SetToPlayerColor(bulletGameObject);
        Bullet bulletScript = bulletGameObject.GetComponent<Bullet>();
        bulletScript.SetOwnerClientID(OwnerClientID);
        
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

    private void OnDestroy()
    {

        foreach (Bullet bullet in BulletList)
        {
            if (bullet == null)
                continue;
            Destroy(bullet.gameObject);
        }

        PlayerManager.OnPlayerDeath -= Disable;
        PlayerManager.OnPlayerRespawn -= Enable;
    }
}
