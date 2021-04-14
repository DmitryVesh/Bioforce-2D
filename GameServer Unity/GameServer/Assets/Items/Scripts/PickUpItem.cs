using GameServer;
using System;
using UnityEngine;

public enum PickupType
{
    bandage,
    medkit,
    adrenaline
}
public class PickupItem : MonoBehaviour
{
    [SerializeField] public PickupType PickupType;
    public ushort PickupID { get; set; }

    public void SetPosition(Vector2 position) =>
        transform.position = position;
    public void SetActive(bool active) =>
        gameObject.SetActive(active);

    protected virtual void PickedUp(PlayerServer player) { }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerServer player = collision.GetComponent<PlayerServer>();
        if (player is null)
            return;

        PickupItem pickup = PickupItemsManager.Instance.PickedUpItem(PickupID);
        if (pickup is null)
            return;

        ServerSend.PlayerPickedUpItem(player.ID, PickupID, pickup);
        PickedUp(player);
    }
}
