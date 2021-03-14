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

    public Action<int> OnPickup { get; set; }
    public int PickupID { get; set; }

    public void SetActive(bool active)
    {
        ItemCollider.enabled = active;
        SpriteRenderer.enabled = active;
    }
    public void SetPosition(Transform tf) =>
        transform.position = tf.position;
    public void PlayEffectsAndHide()
    {
        PickupEffect.Play();
        SetActive(false);
    }

    protected virtual void PlayerPickedUp(PlayerManager player)
    {
        PlayEffectsAndHide();
        OnPickup?.Invoke(PickupID);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerManager player = collision.GetComponentInParent<PlayerManager>();
        if (player is null)
            return;

        PlayerPickedUp(player);        
    }
}
