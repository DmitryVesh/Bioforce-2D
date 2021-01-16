using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum ScoreboardArrayListIndexes
{
    score,
    username,
    kills,
    deaths
}
public class ScoreboardEntry : MonoBehaviour, IUIItemListing
{
    private List<TextMeshProUGUI> ScoreNameKillsDeathsTexts { get; set; }
    
    private TextMeshProUGUI PingText { get; set; }

    private ArrayList ItemList { get; set; }

    public void Init(int score, string username, int kills, int deaths)
    {
        ScoreNameKillsDeathsTexts = new List<TextMeshProUGUI>();
        ScoreNameKillsDeathsTexts.Add(transform.GetChild((int)ScoreboardArrayListIndexes.score).GetComponent<TextMeshProUGUI>());
        ScoreNameKillsDeathsTexts.Add(transform.GetChild((int)ScoreboardArrayListIndexes.username).GetComponent<TextMeshProUGUI>());
        ScoreNameKillsDeathsTexts.Add(transform.GetChild((int)ScoreboardArrayListIndexes.kills).GetComponent<TextMeshProUGUI>());
        ScoreNameKillsDeathsTexts.Add(transform.GetChild((int)ScoreboardArrayListIndexes.deaths).GetComponent<TextMeshProUGUI>());

        PingText = transform.GetChild(4).GetComponent<TextMeshProUGUI>();
        SetText(score, username, kills, deaths);
        SetArrayList(new ArrayList() { score, username, kills, deaths });
    }
    public void SetText(int score, string username, int kills, int deaths)
    {
        ScoreNameKillsDeathsTexts[(int)ScoreboardArrayListIndexes.score].text = score.ToString();
        ScoreNameKillsDeathsTexts[(int)ScoreboardArrayListIndexes.username].text = username;
        ScoreNameKillsDeathsTexts[(int)ScoreboardArrayListIndexes.kills].text = kills.ToString();
        ScoreNameKillsDeathsTexts[(int)ScoreboardArrayListIndexes.deaths].text = deaths.ToString();
    }

    //Interface methods
    public void SetArrayList(ArrayList itemList) =>
        ItemList = itemList;
    public IComparable GetItemInList(int itemListIndex) =>
        (IComparable)ItemList[itemListIndex];
    public GameObject GetGameObject() =>
        gameObject;
    public void AddToItemIndex(int itemListIndex, int toAdd)
    {
        ItemList[itemListIndex] = (int)ItemList[itemListIndex] + toAdd;
        ScoreNameKillsDeathsTexts[itemListIndex].text = ItemList[itemListIndex].ToString();
    }
}