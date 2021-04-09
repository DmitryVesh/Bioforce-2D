using System;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemsManager : MonoBehaviour
{
    public static PickupItemsManager Instance { get; private set; }
    private Transform PickupHolder { get; set; }
    private Dictionary<int, PickupItem> PickupDictionary { get; set; } = new Dictionary<int, PickupItem>();

    [SerializeField] private GameObject Bandage;
    [SerializeField] private GameObject Medkit;

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

    public void SpawnGeneratedPickup(PickupType pickupType, int pickupID, Vector2 position)
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
            default:
                pickupObject = null;
                break;
        }

        PickupItem pickup = Instantiate(pickupObject, PickupHolder).GetComponent<PickupItem>();
        pickup.transform.position = position;
        pickup.PickupID = pickupID;
        pickup.OnPickup += LocalPlayerPickedUpItem;

        PickupDictionary.Add(pickupID, pickup);
    }

    private void LocalPlayerPickedUpItem(int pickupID, int clientID)
    {
        ClientSend.LocalPlayerPickedUpItem(pickupID);
    }

    internal void OtherPlayerPickedUpItem(int pickupID)
    {
        PickupItem pickup = PickupDictionary[pickupID];
        pickup.PlayEffectsAndHide();
        PickupDictionary.Remove(pickupID);
    }
}
