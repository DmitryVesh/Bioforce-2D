using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Instance { get; set; }
    private GameObject ScoreboardPanel { get; set; }

    [SerializeField] private GameObject ScoreboardEntryPrefab; //Set in inspector
    private Dictionary<int, ScoreboardEntry> ScoreboardEntriesDictionary { get; set; } = new Dictionary<int, ScoreboardEntry>();
    private List<ScoreboardEntry> ScoreboardEntriesPanels { get; set; } = new List<ScoreboardEntry>();
    private bool ScoreboardChanged { get; set; }

    private GameObject ScoreboardTimer { get; set; }

    public void AddEntry(int iD, string username, int kills, int deaths, int score)
    {
        GameObject entryToAdd = Instantiate(ScoreboardEntryPrefab, ScoreboardPanel.transform);
        ScoreboardEntry scoreboardEntry = entryToAdd.GetComponent<ScoreboardEntry>();
        scoreboardEntry.Init();
        scoreboardEntry.Set(iD, username, kills, deaths, score);

        ScoreboardEntriesDictionary.Add(iD, scoreboardEntry);
        ScoreboardEntriesPanels.Add(scoreboardEntry);
        ScoreboardChanged = true;
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
        ScoreboardPanel = transform.GetChild(0).gameObject;
        ScoreboardTimer = transform.GetChild(1).gameObject;
        SetActiveScoreboard(false);
    }
    private void Update()
    {
        if (!Client.Instance.Connected)
            return;

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
        /* TODO: implement proper sorting of the entries in the child gameObject.
        int entryCount = ScoreboardEntriesPanels.Count - 1;
        Dictionary<int, ScoreboardEntry> temp = new Dictionary<int, ScoreboardEntry>();
        foreach (KeyValuePair<int, ScoreboardEntry> entryPair in temp)
        {
            ScoreboardEntriesPanels[entryCount].Set(entryPair.Value);
            entryCount--;
        }
        */
    }

    private Dictionary<int, ScoreboardEntry> MergeSortScoreboardPanelEntries(Dictionary<int, ScoreboardEntry> unsortedList)
    {
        //Need to sort the panel transform children objects so the lowest index has highest score
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
                if (left.Value.Score <= right.Value.Score) //left entries score greater or equals to right entries 
                {
                    merged.Add(left.Key, left.Value); //Add left one
                    leftEntries.Remove(left.Key);
                }
                else
                {
                    merged.Add(right.Key, right.Value);
                    rightEntries.Remove(right.Key);
                }
            }
            else if (leftEntries.Count > 0)
            {
                left = leftEntries.First();
                merged.Add(left.Key, left.Value);
                leftEntries.Remove(left.Key);
            }
            else if (rightEntries.Count > 0)
            {
                right = rightEntries.First();
                merged.Add(right.Key, right.Value);
                rightEntries.Remove(right.Key);
            }
        }
        return merged;
    }

    private void SetActiveScoreboard(bool isActive)
    {
        ScoreboardTimer.SetActive(isActive);
        ScoreboardPanel.SetActive(isActive);
    }

    
}
