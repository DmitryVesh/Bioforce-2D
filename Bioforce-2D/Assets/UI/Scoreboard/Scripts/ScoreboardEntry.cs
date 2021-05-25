using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ScoreboardEntryArrayListIndexes
{
    username,
    kills,
    deaths,
    score,
    ping
}
public class ScoreboardEntry : MonoBehaviour, IUIItemListing
{
    [SerializeField] Image Background;

    private List<TextMeshProUGUI> ArrayListTexts { get; set; }
    private ArrayList ItemList { get; set; }

    //TODO: Add pings to the scoreboard entry of players.
    //private TextMeshProUGUI PingText { get; set; }    

    public void Init(byte iD, int score, string username, int kills, int deaths, int pingMS)
    {
        ArrayListTexts = new List<TextMeshProUGUI>();
        ArrayListTexts.Add(transform.GetChild((int)ScoreboardEntryArrayListIndexes.username).GetComponent<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ScoreboardEntryArrayListIndexes.kills).GetComponent<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ScoreboardEntryArrayListIndexes.deaths).GetComponent<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ScoreboardEntryArrayListIndexes.score).GetComponent<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ScoreboardEntryArrayListIndexes.ping).GetComponent<TextMeshProUGUI>());

        //PingText = transform.GetChild(4).GetComponent<TextMeshProUGUI>();
        SetText(score, username, kills, deaths, pingMS);

        Color color = GameManager.PlayerDictionary[iD].PlayerColor;
        color = new Color(color.r, color.g, color.b, (float)100 / (float)255);
        Background.color = color;
    }
    public void SetText(int score, string username, int kills, int deaths, int pingMS)
    {
        ArrayListTexts[(int)ScoreboardEntryArrayListIndexes.username].text = username;
        ArrayListTexts[(int)ScoreboardEntryArrayListIndexes.kills].text = kills.ToString();
        ArrayListTexts[(int)ScoreboardEntryArrayListIndexes.deaths].text = deaths.ToString();
        ArrayListTexts[(int)ScoreboardEntryArrayListIndexes.score].text = score.ToString();
        ArrayListTexts[(int)ScoreboardEntryArrayListIndexes.ping].text = pingMS.ToString();

        SetArrayList(new ArrayList() { username, kills, deaths, score, pingMS});
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
        ArrayListTexts[itemListIndex].text = ItemList[itemListIndex].ToString();
    }
    public void SetPing(int itemListIndex, int pingMS)
    {
        ItemList[itemListIndex] = pingMS;
        ArrayListTexts[itemListIndex].text = ItemList[itemListIndex].ToString();
    }
}