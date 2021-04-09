using System;
using UnityEngine;

public enum PickupType
{
    bandage,
    medkit
}
public abstract class PickupItem : MonoBehaviour
{
    [SerializeField] private PickupType PickupType;
    [SerializeField] private ParticleSystem PickupEffect;
    [SerializeField] private Collider2D ItemCollider;
    [SerializeField] private SpriteRenderer SpriteRenderer;

    public Action<int, int> OnPickup { get; set; }
    public int PickupID { get; set; }

    public void SetActive(bool active)
    {
        ItemCollider.enabled = active;
        SpriteRenderer.enabled = active;
    }
    public void PlayEffectsAndHide()
    {
        PickupEffect.Play();
        //TODO: make an animation e.g. a white circle that collapses into a smaller circle
        SetActive(false);
        Invoke("DestroyItem", 5f);
    }
    private void DestroyItem() //Called by Invoke
    {
        Destroy(PickupEffect.gameObject);
        Destroy(gameObject);
    }

    protected virtual void PlayerPickedUp(PlayerManager player)
    {
        PlayEffectsAndHide();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerManager player = collision.GetComponentInParent<PlayerManager>();
        if (player is null || player.ID != Client.Instance.ClientID) //Must be a local player to pickup the item
            return;

        PlayerPickedUp(player);
        OnPickup?.Invoke(PickupID, player.ID);
    }
}
