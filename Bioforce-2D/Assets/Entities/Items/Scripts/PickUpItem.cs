using System.Collections;
using UnityEngine;

public enum PickupType
{
    bandage,
    medkit,
    adrenaline
}
public abstract class PickupItem : MonoBehaviour
{
    [SerializeField] public PickupType PickupType;
    [SerializeField] private ParticleSystem PickupEffect;
    [SerializeField] private SpriteRenderer SpriteRenderer;

    [SerializeField] private MinimapIcon ItemMinimapIcon;

    public ushort PickupID { get; set; }
    
    public virtual void PlayerPickedUp(PlayerManager player)
    {
        PlayEffectsAndHide();
    }
    public void ResetPickup() =>
        PlayEffectsAndHide();

    private void SetActive(bool active)
    {
        SpriteRenderer.enabled = active;
    }
    private void PlayEffectsAndHide()
    {
        PickupEffect.Play();
        //TODO: make an animation e.g. a white circle that collapses into a smaller circle
        SetActive(false);
        StartCoroutine(DestroyItem());
    }
    private IEnumerator DestroyItem() //Called by Invoke
    {
        Destroy(ItemMinimapIcon);
        yield return new WaitForSeconds(5f);
        Destroy(PickupEffect.gameObject);
        Destroy(gameObject);
    }

    
}
