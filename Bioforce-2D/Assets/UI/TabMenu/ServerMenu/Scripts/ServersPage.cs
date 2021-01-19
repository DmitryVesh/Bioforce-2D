using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServersPage : UIItemListingManager
{
    private Dictionary<int, IUIItemListing> ServerInfoItemLists { get; set; } = new Dictionary<int, IUIItemListing>();
    private bool ServerAdded { get; set; }
    private bool SortByChanged { get; set; }
    private int ServerCount { get; set; } = 0;

    private Queue<(string, int, string, int)> ServersToAdd = new Queue<(string, int, string, int)>();
    public void EnqueEntry(string serverName, int playerCount, string mapName, int ping) =>
        ServersToAdd.Enqueue((serverName, playerCount, mapName, ping));

    //Interface methods
    protected override void SetIndexesToCompareInMergeSort(List<(int, bool)> indexesToCompare) =>
        IndexesToCompare = indexesToCompare;
    protected override void SortTransformsItemListingsDictionary()
    {
        foreach (ServerEntry serverEntry in ServerInfoItemLists.Values)
            serverEntry.transform.SetAsFirstSibling();
    }

    private void Awake()
    {
        SetIndexesToCompareInMergeSort(new List<(int, bool)>()); //By default set to not sort by anything
    }
    private void FixedUpdate()
    {
        if (ServersToAdd.Count != 0)
            AddEntry();

        if (IndexesToCompare.Count != 0 && (ServerAdded || SortByChanged))
        {
            ServerInfoItemLists = MergeSortItemListings(ServerInfoItemLists, IndexesToCompare);
            SortTransformsItemListingsDictionary();
        }

        
    }

    private void AddEntry()
    {
        (string serverName, int playerCount, string mapName, int ping) = ServersToAdd.Dequeue();

        GameObject entryToAdd = Instantiate(ItemListingPrefab, transform);
        ServerEntry serverEntry = entryToAdd.GetComponent<ServerEntry>();
        serverEntry.Init(serverName, playerCount, mapName, ping);

        ServerInfoItemLists.Add(ServerCount, serverEntry);
        ServerCount++;
        ServerAdded = true;
    }
}
