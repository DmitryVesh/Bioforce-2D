using System.Collections.Generic;
using UnityEngine;

public class NonLocalPlayerGun : MonoBehaviour, IGun
{
    [SerializeField] protected GameObject BulletPrefab;
    protected List<Bullet> BulletList;

    private int OwnerClientID = -1;
    protected bool CanShoot = true;
    private PlayerManager PlayerManager { get; set; } = null;


    public virtual void ShootBullet(Vector2 position, Quaternion rotation)
    {
        Bullet bullet;
        bullet = GetBullet();

        if (bullet == null)
            bullet = AddBullet();

        bullet.Shoot(position, rotation);
    }
    public void SetOwnerClientID(int iD)
    {
        OwnerClientID = iD;
    }
    
    public void Disable()
    {
        CanShoot = false;
    }
    public void Enable()
    {
        Invoke("SetCanShootTrue", PlayerManager.RespawnTime);
    }
    private void SetCanShootTrue()
    {
        CanShoot = true;
    }

    private void Awake()
    {
        BulletList = new List<Bullet>();
    }
    private void Start()
    {
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];
        PlayerManager.OnPlayerDeath += Disable;
        PlayerManager.OnPlayerRespawn += Enable;
    }

    private Bullet AddBullet()
    {
        GameObject bulletGameObject = Instantiate(BulletPrefab, Vector3.zero, Quaternion.identity);
        Bullet bulletScript = bulletGameObject.GetComponent<Bullet>();
        bulletScript.SetOwnerClientID(OwnerClientID);
        
        BulletList.Add(bulletScript);
        return bulletScript;
    }
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
        PlayerManager.OnPlayerDeath -= Disable;
        PlayerManager.OnPlayerRespawn -= Enable;
    }
}
