using GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;

public class PickupItemsManager : MonoBehaviour
{
    public static PickupItemsManager Instance { get => instance; }
    private static PickupItemsManager instance;

    
    [SerializeField] private GameObject Bandage;
    [SerializeField] private GameObject Medkit;
    [SerializeField] private GameObject Adrenaline;

    [SerializeField] private byte MaxNumPickups = 10;
    [SerializeField] private double TimeInBetweenPickupSpawnsMS = 10_000d; //10s in ms
    private Timer SpawnPickupsTimer { get; set; }

    private Queue<PickupItem> PickupsAvailable { get; set; } = new Queue<PickupItem>();
    public Dictionary<ushort, PickupItem> PickupsDictionary { get; private set; } = new Dictionary<ushort, PickupItem>();
    private ushort NumPickupsExistedCount { get; set; } = 0;

    [SerializeField] List<Transform> SpawnLocations;
    private List<Vector2> SpawnLocationUsing { get; set; } = new List<Vector2>();

    private void Start()
    {
        GameStateManager.Instance.OnServerGameActivated += StartSpawningItems;
        GameStateManager.Instance.OnServerGameEnded += StopSpawningItems;
        GameStateManager.Instance.OnServerRestart += StopSpawningItems;
        GameStateManager.Instance.OnServerRestart += ResetAllItems;

        Transform pickupHolder = new GameObject("PickupHolder").transform;
        pickupHolder.position = Vector3.zero;

        //Adding defaults
        MakeAndAddPickupToAvailableQueue(pickupHolder, Bandage);
        MakeAndAddPickupToAvailableQueue(pickupHolder, Medkit);
        MakeAndAddPickupToAvailableQueue(pickupHolder, Adrenaline);
        MakeAndAddPickupToAvailableQueue(pickupHolder, Bandage);
        MakeAndAddPickupToAvailableQueue(pickupHolder, Medkit);
        MakeAndAddPickupToAvailableQueue(pickupHolder, Bandage);

        for (byte pickupCount = 0; pickupCount < MaxNumPickups * 2; pickupCount++)
            MakeAndAddPickupToAvailableQueue(pickupHolder, GetRandomPickupGameObject());
    }
    private void OnDestroy()
    {
        GameStateManager.Instance.OnServerGameActivated -= StartSpawningItems;
        GameStateManager.Instance.OnServerGameEnded -= StopSpawningItems;
        GameStateManager.Instance.OnServerRestart -= StopSpawningItems;
        GameStateManager.Instance.OnServerRestart -= ResetAllItems;
    }


    private void Awake()
    {
        Singleton.Init(ref instance, this);
        
    }
    

    private void StartSpawningItems(float _)
    {
        SpawnPickupsTimer = new Timer(TimeInBetweenPickupSpawnsMS);
        SpawnPickupsTimer.Elapsed += GeneratePickup;
        SpawnPickupsTimer.Start();
        Output.WriteLine("\t!Started spawning items timer!");
    }
    private void StopSpawningItems()
    {
        SpawnPickupsTimer.Stop();
        Output.WriteLine("\t!Stopped spawning items timer!");
    }
    private void ResetAllItems()
    {
        ushort[] pickupIDs = PickupsDictionary.Keys.ToArray();
        foreach (ushort itemID in pickupIDs)
            PickedUpItem(itemID);

    }

    private void MakeAndAddPickupToAvailableQueue(Transform pickupHolder, GameObject pickupObject)
    {
        PickupItem pickup = Instantiate(pickupObject, pickupHolder).GetComponent<PickupItem>();
        AddPickupToAvailableQueue(pickup);
    }

    private void AddPickupToAvailableQueue(PickupItem pickup)
    {
        Output.WriteLine($"\tInstantiated a Pickup of type: {pickup.PickupType}");
        PickupsAvailable.Enqueue(pickup);
        pickup.SetActive(false);
    }

    private void AddPickupToDictionary(PickupItem pickup)
    {
        pickup.SetPosition(GetRandomPickupSpawnPoint());
        pickup.PickupID = NumPickupsExistedCount;

        PickupsDictionary.Add(NumPickupsExistedCount++, pickup);
    }

    private Vector2 GetRandomPickupSpawnPoint()
    {
        Vector2 spawnLocation;
        do
        {
            spawnLocation = SpawnLocations[UnityEngine.Random.Range(0, SpawnLocations.Count)].position;
        } while (SpawnLocationUsing.Contains(spawnLocation));

        SpawnLocationUsing.Add(spawnLocation);
        return spawnLocation;
    }

    public PickupItem PickedUpItem(ushort pickupID)
    {
        //TODO: 9005 deal with if another player already has pickedUp the item...
        PickupItem pickup;
        if (!PickupsDictionary.TryGetValue(pickupID, out pickup))
        {
            //Output.WriteLine("" +
            //    "\n\t===================================================================" +
            //    "\n\tPickup that doesn't exist in dictionary on GameServer was picked up" +
            //    "\n\t===================================================================");
            return null;
        }

        pickup.SetActive(false);

        PickupsAvailable.Enqueue(pickup);
        PickupsDictionary.Remove(pickupID);
        SpawnLocationUsing.Remove(pickup.transform.position);

        return pickup;
    }

    private GameObject GetRandomPickupGameObject()
    {
        float dropRarity = UnityEngine.Random.Range(0f, 1f);
        if (dropRarity < 0.15f)
            return Adrenaline;
        else if (dropRarity < 0.4f)
            return Medkit;
        else
            return Bandage;
    }
    private void GeneratePickup(object sender, ElapsedEventArgs e)
    {
        if (PickupsDictionary.Count >= MaxNumPickups)
            return;

        PickupItem pickup = PickupsAvailable.Dequeue();
        pickup.SetActive(true);
        AddPickupToDictionary(pickup);
        ServerSend.GeneratedPickupItem(pickup);
    }
}
