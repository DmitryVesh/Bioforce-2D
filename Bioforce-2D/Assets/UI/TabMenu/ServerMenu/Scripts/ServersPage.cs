using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ServersPage : UIItemListingManager
{
    [SerializeField] private Color ServerEntryHoverColor;
    [SerializeField] private Color ServerEntrySelectColor;
    [SerializeField] private Color ServerEntryExitColor;
    private ServerEntry SelectedEntry { get; set; }

    private Dictionary<object, IUIItemListing> ServerInfoItemLists { get; set; } = new Dictionary<object, IUIItemListing>();
    private bool ServerAdded { get; set; }
    private bool SortByChanged { get; set; }

    [SerializeField] private GameObject NoConnectionToMainServerEntryPrefab;
    [SerializeField] private GameObject NoServersFoundEntryPrefab;
    

    private Queue<(string, int, int, string, int)> ServersToAdd = new Queue<(string, int, int, string, int)>();
    public void EnqueEntry(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping) =>
        ServersToAdd.Enqueue((serverName, currentPlayerCount, maxPlayerCount, mapName, ping));
    internal void LostConnectionToMainServer()
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            NoServersFoundEntryPrefab.SetActive(false);
            DeleteAllServers();
            NoConnectionToMainServerEntryPrefab.SetActive(true);
        });
    }
    internal void RegainedConnectionToMainServer()
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            NoConnectionToMainServerEntryPrefab.SetActive(false);
            NoServersFoundEntryPrefab.SetActive(true);
        });
    }
    public void DeleteServer(string serverName)
    {
        try
        {
            Destroy(ServerInfoItemLists[serverName].GetGameObject());
            ServerInfoItemLists.Remove(serverName);

            if (ServerInfoItemLists.Count == 0)
                NoServersFoundEntryPrefab.SetActive(true);
        }
        catch (KeyNotFoundException) {}
    }
    private void DeleteAllServers()
    {
        foreach (ServerEntry serverEntry in ServerInfoItemLists.Values)
            Destroy(serverEntry.GetGameObject());
        ServerInfoItemLists.Clear();
    }

    public void ModifyServer(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping)
    {
        if (ServerInfoItemLists.ContainsKey(serverName))
            ((ServerEntry)ServerInfoItemLists[serverName]).SetText(serverName, currentPlayerCount, maxPlayerCount, mapName, ping);
    }

    public void OnServerEntryHover(ServerEntry serverEntry)
    {
        ResetNonSelectedEntries();
        if (serverEntry != SelectedEntry)
            serverEntry.SetBackgroundColor(ServerEntryHoverColor);
    }
    public void OnServerEntrySelect(ServerEntry serverEntry)
    {
        serverEntry.SetBackgroundColor(ServerEntrySelectColor);
        SelectedEntry = serverEntry;
        ServerMenu.SetEntrySelected(SelectedEntry);
        ResetNonSelectedEntries();
    }
    public void OnServerEntryExit(ServerEntry serverEntry)
    {
        ResetNonSelectedEntries();
    }

    private void ResetNonSelectedEntries()
    {
        foreach (ServerEntry serverEntry in ServerInfoItemLists.Values)
        {
            if (serverEntry != SelectedEntry)
                ResetEntry(serverEntry);
        }
    }
    private void ResetEntry(ServerEntry serverEntry) =>
        serverEntry.SetBackgroundColor(ServerEntryExitColor);

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
        NoConnectionToMainServerEntryPrefab.SetActive(false);
        NoServersFoundEntryPrefab.SetActive(true);
    }
    private void FixedUpdate()
    {
        if (ServersToAdd.Count != 0)
            AddEntry();

        if (IndexesToCompare.Count != 0 && (ServerAdded || SortByChanged))
        {
            ServerInfoItemLists = MergeSortItemListings(ServerInfoItemLists, IndexesToCompare);
            SortTransformsItemListingsDictionary();
            ServerAdded = false;
            SortByChanged = false;
        }

        
    }

    private void AddEntry()
    {
        (string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping) = ServersToAdd.Dequeue();
        if (ServerInfoItemLists.ContainsKey(serverName))
            return;

        NoConnectionToMainServerEntryPrefab.SetActive(false);
        NoServersFoundEntryPrefab.SetActive(false);

        
        Debug.Log($"Adding server: {serverName}, to server page");
        GameObject entryToAdd = Instantiate(ItemListingPrefab, transform);
        ServerEntry serverEntry = entryToAdd.GetComponent<ServerEntry>();
        serverEntry.Init(this, serverName, currentPlayerCount, maxPlayerCount, mapName, ping);

        ServerInfoItemLists.Add(serverName, serverEntry);
        ServerAdded = true;
    }

    
}
