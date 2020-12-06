using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileLocalPlayerGun : LocalPlayerGun
{
    private bool PlayerTriedToShoot { get; set; } = false;

    protected override void Update()
    {
        for (int touchCount = 0; touchCount < Input.touchCount; touchCount++)
        {
            Touch touch = Input.GetTouch(touchCount);
            if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width / 2)
            {
                SetPlayerTriedToShootTrue();
                break;
            }
        }
        base.Update();
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
