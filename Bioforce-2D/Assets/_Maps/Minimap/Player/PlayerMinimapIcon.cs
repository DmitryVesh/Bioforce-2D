using System.Collections;
using UnityEngine;

public class PlayerMinimapIcon : MinimapIcon
{
    private Animator Anim { get; set; }
    private PlayerManager PlayerManager { get; set; }
    protected override void Start()
    {
        base.Start();
        //Important that the Anim is set after the base.Start, because GameObject with the Anim is insantiated in Minimap Call
        Anim = Icon.gameObject.GetComponent<Animator>();
        
        PlayerManager = GetComponentInParent<PlayerManager>();
        PlayerManager.OnPlayerInvincibility += FlashPlayerIcon;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        PlayerManager.OnPlayerInvincibility -= FlashPlayerIcon;
    }

    private Coroutine FlashingIcon = null;
    private void FlashPlayerIcon(float timeToFlashForSec)
    {
        if (!(FlashingIcon is null))
            StopCoroutine(FlashingIcon);

        FlashingIcon = StartCoroutine(FlashIcon(timeToFlashForSec * PlayerManager.TimeBeforeStoppingFlashMultiplier));
    }
    private IEnumerator FlashIcon(float timeToFlashSec)
    {
        Anim.SetBool("Flash", true);
        yield return new WaitForSeconds(timeToFlashSec);
        Anim.SetBool("Flash", false);
    }
}
