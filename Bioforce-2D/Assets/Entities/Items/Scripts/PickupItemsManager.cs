using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class PickupItemsManager : HostStateManager
{
    /*
    Make a HostClient Singleton class, which messages all of the HostStateManager classes, 
        to control (e.g. spawn health pickups),
        or to listen to traffic coming from GameServer, which actually comes from another HostClient who's in control, and act accordinly

    Make a HostStateManager  abstract inheritable class, which either controls Managers like PickupItemsManager, to spawn in stuff and send to the GameServer,        
    */

    [SerializeField] private GameObject Bandage;
    [SerializeField] private GameObject Medkit;

    [SerializeField] private int MaxNumPickups = 10;
    private int CurrentNumPickups { get; set; }
    [SerializeField] private double TimeInBetweenPickupSpawns = 10d;
    private Timer SpawnPickupsTimer { get; set; }

    private Queue<PickupItem> PickupsAvailable { get; set; } = new Queue<PickupItem>();
    private Dictionary<int, PickupItem> PickupsDictionary { get; set; } = new Dictionary<int, PickupItem>();
    private int NumPickupsExistedCount { get; set; } = 0;
    [SerializeField] Queue<Transform> SpawnLocations; //TODO: 9000 Decide if want a Queue or random system...

    private void Awake()
    {
        SpawnPickupsTimer = new Timer(TimeInBetweenPickupSpawns);
        SpawnPickupsTimer.Elapsed += GeneratePickup;

        Transform pickupHolder = new GameObject("PickupHolder").transform;
        pickupHolder.position = Vector3.zero;

        for (int pickupCount = 0; pickupCount < MaxNumPickups; pickupCount++)
        {
            PickupItem pickup1 = AddPickupToAvailableQueue(pickupHolder);
            PickupItem pickup2 = AddPickupToAvailableQueue(pickupHolder);

            AddPickupToDictionary(pickup1);
        }
        CurrentNumPickups = MaxNumPickups;
    }

    private PickupItem AddPickupToAvailableQueue(Transform pickupHolder)
    {
        PickupItem pickup = Instantiate(GetRandomPickupGameObject(), pickupHolder).GetComponent<PickupItem>();
        pickup.OnPickup += RemoveFromDict;
        pickup.SetActive(false);

        PickupsAvailable.Enqueue(pickup);     
        return pickup;
    }
    private void AddPickupToDictionary(PickupItem pickup)
    {
        pickup.PickupID = NumPickupsExistedCount;
        pickup.SetActive(true);
        //pickup.SetPosition(); //TODO: 9001 Set the position of the pick up item

        PickupsDictionary.Add(NumPickupsExistedCount++, pickup);
    }

    private void RemoveFromDict(int iD)
    {
        PickupsAvailable.Enqueue(PickupsDictionary[iD]);
        PickupsDictionary.Remove(iD);
    }

    private GameObject GetRandomPickupGameObject()
    {
        float dropRarity = Random.Range(0f, 1f);
        if (dropRarity < 0.4f)
            return Medkit;
        else
            return Bandage;
    }
    private void GeneratePickup(object sender, ElapsedEventArgs e)
    {
        if (!Hosting && CurrentNumPickups >= MaxNumPickups)
            return;

        PickupItem pickup = PickupsAvailable.Dequeue();
        AddPickupToDictionary(pickup);
    }
}
