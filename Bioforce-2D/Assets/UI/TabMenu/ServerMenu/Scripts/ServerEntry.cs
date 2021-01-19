using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ServerEntryArrayListIndexes
{
    serverName,
    playerCount,
    mapName,
    ping
}
public class ServerEntry : MonoBehaviour, IUIItemListing
{
    private List<TextMeshProUGUI> ArrayListTexts { get; set; }
    private ArrayList ItemList { get; set; }

    public void Init(string serverName, int playerCount, string mapName, int ping)
    {
        ArrayListTexts = new List<TextMeshProUGUI>();
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.serverName).GetComponent<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.playerCount).GetComponent<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.mapName).GetComponent<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.ping).GetComponent<TextMeshProUGUI>());

        SetText(serverName, playerCount, mapName, ping);
    }
    public void SetText(string serverName, int playerCount, string mapName, int ping)
    {
        ArrayListTexts[(int)ServerEntryArrayListIndexes.serverName].text = serverName;
        ArrayListTexts[(int)ServerEntryArrayListIndexes.playerCount].text = playerCount.ToString();
        ArrayListTexts[(int)ServerEntryArrayListIndexes.mapName].text = mapName;
        ArrayListTexts[(int)ServerEntryArrayListIndexes.ping].text = ping.ToString();

        SetArrayList(new ArrayList() { serverName, playerCount, mapName, ping });
    }

    //Interface methods
    public void AddToItemIndex(int itemListIndex, int toAdd) //Unused in this class
    {
        ItemList[itemListIndex] = (int)ItemList[itemListIndex] + toAdd;
        ArrayListTexts[itemListIndex].text = ItemList[itemListIndex].ToString();
    }

    public GameObject GetGameObject() =>
        gameObject;
    public IComparable GetItemInList(int itemListIndex) =>
        (IComparable)ItemList[itemListIndex];
    public void SetArrayList(ArrayList itemList) =>
        ItemList = itemList;
}
