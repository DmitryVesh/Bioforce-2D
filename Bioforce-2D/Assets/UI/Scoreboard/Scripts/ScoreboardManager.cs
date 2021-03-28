using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardManager : UIItemListingManager
{
    public static ScoreboardManager Instance { get; set; }
    private GameObject Scoreboard { get; set; }
    private GameObject ScoreboardPanel { get; set; }

    private Dictionary<object, IUIItemListing> PlayersItemLists { get; set; } = new Dictionary<object, IUIItemListing>();
    private bool ScoreboardChanged { get; set; }

    private GameObject ScoreboardTimer { get; set; }
    private Button MobileButton { get; set; }
    private bool ScoreboardActive { get; set; }
    private bool ClickedOdd { get; set; }

    public void AddEntry(int iD, string username, int kills, int deaths, int score)
    {
        GameObject entryToAdd = Instantiate(ItemListingPrefab, ScoreboardPanel.transform);
        ScoreboardEntry scoreboardEntry = entryToAdd.GetComponent<ScoreboardEntry>();
        scoreboardEntry.Init(iD, score, username, kills, deaths);

        PlayersItemLists.Add(iD, scoreboardEntry);
        ScoreboardChanged = true;
    }
    internal void DeleteEntry(int disconnectedPlayer)
    {
        try
        {
            Destroy(PlayersItemLists[disconnectedPlayer].GetGameObject());
            PlayersItemLists.Remove(disconnectedPlayer);
        }
        catch (KeyNotFoundException) {}
    }
    public void AddKill(int bulletOwnerID)
    {
        PlayersItemLists[bulletOwnerID].AddToItemIndex((int)ScoreboardEntryArrayListIndexes.kills, 1);
        PlayersItemLists[bulletOwnerID].AddToItemIndex((int)ScoreboardEntryArrayListIndexes.score, 3);
        ScoreboardChanged = true;
    }
    public void AddDeath(int ownerClientID)
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
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"ScoreboardManager instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }
        Scoreboard = transform.GetChild(0).gameObject;
        ScoreboardPanel = Scoreboard.transform.GetChild(0).gameObject;
        ScoreboardTimer = Scoreboard.transform.GetChild(2).gameObject;
        SetActiveScoreboard(false);

        MobileButton = transform.GetChild(1).GetComponent<Button>();
        
        if (GameManager.Instance.IsMobileSupported)
            MobileButton.gameObject.SetActive(true);
        else
            MobileButton.gameObject.SetActive(false);

        SetIndexesToCompareInMergeSort(new List<(int, bool)>() { ((int)ScoreboardEntryArrayListIndexes.score, true), ((int)ScoreboardEntryArrayListIndexes.deaths, false) });
    }
    private void Update()
    {
        if (!Client.Instance.Connected)
        {
            MobileButton.interactable = false;
            return;
        }

        MobileButton.interactable = true;
        if (Input.GetButtonDown("Scoreboard"))
            SetActiveScoreboard(true);
        else if (Input.GetButtonUp("Scoreboard"))
            SetActiveScoreboard(false);
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
