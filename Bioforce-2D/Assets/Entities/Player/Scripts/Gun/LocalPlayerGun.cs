using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerGun : NonLocalPlayerGun
{
    [SerializeField] private Transform FirePointTransform;

    private void Update()
    {
        //TODO: add automatic shooting GetButton instead of Down
        if (Input.GetButtonDown("Fire1"))
            ShootBullet();
    }
    private void ShootBullet()
    {
        if (!CanShoot)
            return;
        base.ShootBullet(FirePointTransform.position, FirePointTransform.rotation);
        ClientSend.ShotBullet(FirePointTransform.position, FirePointTransform.rotation);
    }

    
}
