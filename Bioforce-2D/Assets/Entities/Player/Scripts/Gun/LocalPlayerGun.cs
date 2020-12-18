using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerGun : NonLocalPlayerGun
{
    [SerializeField] private Transform FirePointTransform; //Set in inspector

    protected virtual bool IsPlayerTryingToShoot()
    {
        //TODO: add automatic shooting GetButton instead of Down
        return Input.GetButtonDown("Fire1");
    }
    protected virtual void Update()
    {
        if (IsPlayerTryingToShoot())
            ShootBullet();
    }
    private void ShootBullet()
    {
        if (!CanShoot)
            return;
        PlayerManager.CallOnBulletShotEvent(FirePointTransform.position, FirePointTransform.rotation);
        //base.ShootBullet(FirePointTransform.position, FirePointTransform.rotation);
        ClientSend.ShotBullet(FirePointTransform.position, FirePointTransform.rotation);
        
    }

    
}
