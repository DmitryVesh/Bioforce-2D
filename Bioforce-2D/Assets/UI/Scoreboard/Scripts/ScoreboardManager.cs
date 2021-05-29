using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;
using UnityEngine.UI;

public class ScoreboardManager : UIItemListingManager
{
    public static ScoreboardManager Instance { get => instance; }
    private static ScoreboardManager instance;

    [SerializeField] private GameObject Scoreboard;
    [SerializeField] private GameObject ScoreboardPanel;

    private Dictionary<object, IUIItemListing> PlayersItemLists { get; set; } = new Dictionary<object, IUIItemListing>();
    private bool ScoreboardChanged { get; set; }

    [SerializeField] private Button MobileButton;
    private bool ScoreboardActive { get; set; }
    private bool ClickedOdd { get; set; }

    public void AddEntry(byte iD, string username, int kills, int deaths, int score, int pingMS)
    {
        GameObject entryToAdd = Instantiate(ItemListingPrefab, ScoreboardPanel.transform);
        ScoreboardEntry scoreboardEntry = entryToAdd.GetComponent<ScoreboardEntry>();
        scoreboardEntry.Init(iD, score, username, kills, deaths, pingMS);

        PlayersItemLists.Add(iD, scoreboardEntry);
        ScoreboardChanged = true;
    }
    internal void DeleteEntry(byte disconnectedPlayer)
    {
        try
        {
            Destroy(PlayersItemLists[disconnectedPlayer].GetGameObject());
            PlayersItemLists.Remove(disconnectedPlayer);
        }
        catch (KeyNotFoundException) {}
    }
    public void SetPing(byte clientID, int pingMS)
    {
        ((ScoreboardEntry)PlayersItemLists[clientID]).SetPing((int)ScoreboardEntryArrayListIndexes.ping, pingMS);
        //ScoreboardChanged = true; //Don't need as it doesn't change the sorting of the board
    }
    public void AddKill(byte bulletOwnerID)
    {
        PlayersItemLists[bulletOwnerID].AddToItemIndex((int)ScoreboardEntryArrayListIndexes.kills, 1);
        PlayersItemLists[bulletOwnerID].AddToItemIndex((int)ScoreboardEntryArrayListIndexes.score, 3);
        ScoreboardChanged = true;
    }
    public void AddDeath(byte ownerClientID)
    {
        PlayersItemLists[ownerClientID].AddToItemIndex((int)ScoreboardEntryArrayListIndexes.deaths, 1);
        ScoreboardChanged = true;
    }
    public void ChangedScoreboardActivity() //Called by OnClickEvent by MobileButton
    {
        ClickedOdd = !ClickedOdd;
        if (ClickedOdd)
            SetActiveScoreboard(!ScoreboardActive);
    }
    public void DeleteAllEntries()
    {
        object[] playerIDs = PlayersItemLists.Keys.ToArray();
        foreach (object id in playerIDs)
            DeleteEntry((byte)id);
    }

    //Interface methods
    protected override void SortTransformsItemListingsDictionary()
    {
        foreach (ScoreboardEntry scoreboardEntry in PlayersItemLists.Values)
            scoreboardEntry.transform.SetAsFirstSibling();
    }
    protected override void SetIndexesToCompareInMergeSort(List<(int, bool)> indexesToCompare) =>
        IndexesToCompare = indexesToCompare;

    private void Awake()
    {
        Singleton.Init(ref instance, this);

        SetActiveScoreboard(false);        
        
        MobileButton.gameObject.SetActive(true);

        SetIndexesToCompareInMergeSort(new List<(int, bool)>() 
        { 
            ((int)ScoreboardEntryArrayListIndexes.score, true), 
            ((int)ScoreboardEntryArrayListIndexes.deaths, false) 
        });

        GameStateManager.GameEnded += OpenGameEndingScoreboard;
        GameStateManager.GameRestarting += CloseGameEndingScoreboard;
        GameStateManager.GameRestarting += DeleteAllEntries;
    }
    private void Start()
    {
        ClearAllScoreboardEntries();
    }
    private void ClearAllScoreboardEntries()
    {
        Transform scoreboardTF = ScoreboardPanel.transform;
        int childCount = scoreboardTF.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = scoreboardTF.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy()
    {
        GameStateManager.GameEnded -= OpenGameEndingScoreboard;
        GameStateManager.GameRestarting -= CloseGameEndingScoreboard;
        GameStateManager.GameRestarting -= DeleteAllEntries;
    }

    private void CloseGameEndingScoreboard() =>
        SetActiveScoreboard(false);
    private void OpenGameEndingScoreboard() =>
        SetActiveScoreboard(true);
    
    private void Update()
    {
        if (!Client.Instance.Connected || GameStateManager.CurrentState.Equals(GameState.Ended))
        {
            MobileButton.interactable = false;
            return;
        }

        MobileButton.interactable = true;

#if UNITY_STANDALONE
        if (Input.GetButtonDown("Scoreboard"))
            SetActiveScoreboard(true);
        else if (Input.GetButtonUp("Scoreboard"))
            SetActiveScoreboard(false);
#endif
    }
    private void FixedUpdate()
    {
        if (ScoreboardChanged)
        {
            PlayersItemLists = MergeSortItemListings(PlayersItemLists, IndexesToCompare);
            SortTransformsItemListingsDictionary();
            ScoreboardChanged = false;
        }
    }
    private void SetActiveScoreboard(bool isActive)
    {
        Scoreboard.SetActive(isActive);
        ScoreboardActive = isActive;
    }
}
