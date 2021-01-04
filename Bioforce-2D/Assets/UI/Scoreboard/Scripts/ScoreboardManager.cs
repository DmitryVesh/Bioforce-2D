using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Instance { get; set; }
    private GameObject Scoreboard { get; set; }
    private GameObject ScoreboardPanel { get; set; }

    [SerializeField] private GameObject ScoreboardEntryPrefab; //Set in inspector
    private Dictionary<int, ScoreboardEntry> ScoreboardEntriesDictionary { get; set; } = new Dictionary<int, ScoreboardEntry>();
    private bool ScoreboardChanged { get; set; }

    private GameObject ScoreboardTimer { get; set; }
    private Button MobileButton { get; set; }
    private bool ScoreboardActive { get; set; }
    private bool ClickedOdd { get; set; }

    public void AddEntry(int iD, string username, int kills, int deaths, int score)
    {
        GameObject entryToAdd = Instantiate(ScoreboardEntryPrefab, ScoreboardPanel.transform);
        ScoreboardEntry scoreboardEntry = entryToAdd.GetComponent<ScoreboardEntry>();
        scoreboardEntry.Init();
        scoreboardEntry.Set(iD, username, kills, deaths, score);

        ScoreboardEntriesDictionary.Add(iD, scoreboardEntry);
        ScoreboardChanged = true;
    }
    internal void DeleteEntry(int disconnectedPlayer)
    {
        //TODO: instead of destroying gameObject, should setActive(false), and re-use it later
        try
        {
            Destroy(ScoreboardEntriesDictionary[disconnectedPlayer].gameObject);
            ScoreboardEntriesDictionary.Remove(disconnectedPlayer);
        }
        catch (KeyNotFoundException) {}
    }
    public void AddKill(int bulletOwnerID)
    {
        ScoreboardEntriesDictionary[bulletOwnerID].AddKill();
        ScoreboardChanged = true;
    }
    public void AddDeath(int ownerClientID)
    {
        ScoreboardEntriesDictionary[ownerClientID].AddDeath();
        ScoreboardChanged = true;
    }
    public void ChangedScoreboardActivity() //Called by OnClickEvent by MobileButton
    {
        ClickedOdd = !ClickedOdd;
        if (ClickedOdd)
            SetActiveScoreboard(!ScoreboardActive);
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"ScoreboardManager instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
        Scoreboard = transform.GetChild(0).gameObject;
        ScoreboardPanel = Scoreboard.transform.GetChild(0).gameObject;
        ScoreboardTimer = Scoreboard.transform.GetChild(2).gameObject;
        SetActiveScoreboard(false);

        MobileButton = transform.GetChild(1).GetComponent<Button>();
        
        if (GameManager.Instance.IsMobileSupported())
            MobileButton.gameObject.SetActive(true);
        else
            MobileButton.gameObject.SetActive(false);
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
            ScoreboardEntriesDictionary = MergeSortScoreboardPanelEntries(ScoreboardEntriesDictionary);
            SortTheChildrenOfScoreboardPanel();
            ScoreboardChanged = false;
        }
    }

    private void SortTheChildrenOfScoreboardPanel()
    {
        foreach (ScoreboardEntry scoreboardEntry in ScoreboardEntriesDictionary.Values)
            scoreboardEntry.transform.SetAsFirstSibling();
    }

    private Dictionary<int, ScoreboardEntry> MergeSortScoreboardPanelEntries(Dictionary<int, ScoreboardEntry> unsortedList)
    {
        if (unsortedList.Count <= 1)
            return unsortedList;

        Dictionary<int, ScoreboardEntry> leftEntries = new Dictionary<int, ScoreboardEntry>();
        Dictionary<int, ScoreboardEntry> rightEntries = new Dictionary<int, ScoreboardEntry>();

        int middleIndex = unsortedList.Count / 2;
        int count = 0;
        foreach (KeyValuePair<int, ScoreboardEntry> keyValuePair in unsortedList)
        {
            if (count < middleIndex)
                leftEntries.Add(keyValuePair.Key, keyValuePair.Value);
            else
                rightEntries.Add(keyValuePair.Key, keyValuePair.Value);
            count++;
        }

        leftEntries = MergeSortScoreboardPanelEntries(leftEntries);
        rightEntries = MergeSortScoreboardPanelEntries(rightEntries);

        return MergeScoreboardPanelEntries(leftEntries, rightEntries);
        
    }
    private Dictionary<int, ScoreboardEntry> MergeScoreboardPanelEntries(Dictionary<int, ScoreboardEntry> leftEntries, Dictionary<int, ScoreboardEntry> rightEntries)
    {
        Dictionary<int, ScoreboardEntry> merged = new Dictionary<int, ScoreboardEntry>();

        while (leftEntries.Count > 0 || rightEntries.Count > 0)
        {
            KeyValuePair<int, ScoreboardEntry> left;
            KeyValuePair<int, ScoreboardEntry> right;

            if (leftEntries.Count > 0 && rightEntries.Count > 0) //Both lists have entries to merge
            {
                left = leftEntries.First();
                right = rightEntries.First();

                if (left.Value.Score < right.Value.Score) //left entries score lesser 
                    AddToMerged(left, ref leftEntries, ref merged);

                else if (left.Value.Score > right.Value.Score) // right entries score is lesser
                    AddToMerged(right, ref rightEntries, ref merged);
                
                else // Same score, have to sort by number of deaths
                {
                    if (left.Value.Deaths <= right.Value.Deaths) //The player with most deaths is added first.
                        AddToMerged(right, ref rightEntries, ref merged);
                    else
                        AddToMerged(left, ref leftEntries, ref merged);
                }
            }
            else if (leftEntries.Count > 0)
            {
                left = leftEntries.First();
                AddToMerged(left, ref leftEntries, ref merged);
            }
            else if (rightEntries.Count > 0)
            {
                right = rightEntries.First();
                AddToMerged(right, ref rightEntries, ref merged);
            }
        }
        return merged;
    }
    private void AddToMerged(KeyValuePair<int, ScoreboardEntry> entry, ref Dictionary<int, ScoreboardEntry> entries, ref Dictionary<int, ScoreboardEntry> merged)
    {
        merged.Add(entry.Key, entry.Value);
        entries.Remove(entry.Key);
    }

    private void SetActiveScoreboard(bool isActive)
    {
        Scoreboard.SetActive(isActive);
        ScoreboardActive = isActive;
    }

    
}
