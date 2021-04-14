using System;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemsManager : MonoBehaviour
{
    public static PickupItemsManager Instance { get; private set; }
    private Transform PickupHolder { get; set; }
    private Dictionary<ushort, PickupItem> PickupDictionary { get; set; } = new Dictionary<ushort, PickupItem>();

    [SerializeField] private GameObject Bandage;
    [SerializeField] private GameObject Medkit;
    [SerializeField] private GameObject Adrenaline;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"PickupItemsManager instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }

        PickupHolder = new GameObject("PickupHolder").transform;
        PickupHolder.position = Vector3.zero;
    }

    public void SpawnGeneratedPickup(PickupType pickupType, ushort pickupID, Vector2 position)
    {
        GameObject pickupObject;
        switch (pickupType)
        {
            case PickupType.bandage:
                pickupObject = Bandage;
                break;
            case PickupType.medkit:
                pickupObject = Medkit;
                break;
            case PickupType.adrenaline:
                pickupObject = Adrenaline;
                break;
            default:
                pickupObject = null;
                break;
        }

        GameObject newPickupObj = Instantiate(pickupObject, PickupHolder);
        newPickupObj.transform.position = position;

        PickupItem pickup = newPickupObj.GetComponentInChildren<PickupItem>();
        
        pickup.PickupID = pickupID;

        PickupDictionary.Add(pickupID, pickup);
    }

    internal void PlayerPickedUpItem(ushort pickupID, byte clientWhoPickedUp, Packet packet)
    {
        PickupItem pickup = PickupDictionary[pickupID];
        PickupDictionary.Remove(pickupID);

        switch (pickup.PickupType)
        {
            case PickupType.bandage:
            case PickupType.medkit:
                int healAmount = packet.ReadInt();
                ((HealthPickup)pickup).Restore = healAmount;
                break;
            case PickupType.adrenaline:
                //TimeSpan timeOfInvincibilityStart = packet.ReadTimeSpan();
                float oldInvincibilityTime = packet.ReadFloat();

                //Turns out that the time set on the machines is different, hence why my client
                // showed that there was negative latency... because my time of day was 0.5s slower,
                // showing that dif was -0.5...s 
                // Therefore would be easy to cheat, by changing the time on the device
                //TimeSpan dif = (DateTime.UtcNow.TimeOfDay - timeOfInvincibilityStart);
                //float newInvincibilityTime = oldInvincibilityTime - (float)dif.TotalSeconds;

                //Debug.Log(DateTime.UtcNow.TimeOfDay);
                //Debug.Log(timeOfInvincibilityStart);
                //Debug.Log(dif);
                //Debug.Log(newInvincibilityTime);

                oldInvincibilityTime = oldInvincibilityTime - Client.Instance.LatestLatency1WaySecondsTCP;

                ((AdrenalinePickup)pickup).InvincibilityTime = oldInvincibilityTime;
                break;
        }
        pickup.PlayerPickedUp(GameManager.PlayerDictionary[clientWhoPickedUp]);
    }
}
