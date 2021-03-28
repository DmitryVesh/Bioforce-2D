using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ServerEntryArrayListIndexes
{
    serverName,
    playerCount,
    mapName,
    ping
}
public class ServerEntry : MonoBehaviour, IUIItemListing, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public string MapName { get; private set; }
    public string ServerName { get; private set; }

    private List<TextMeshProUGUI> ArrayListTexts { get; set; }
    private Image TextBackground { get; set; }
    private ArrayList ItemList { get; set; }
    private ServersPage ParentServersPage { get; set; }

    public void Init(ServersPage parentPage, string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping)
    {
        ParentServersPage = parentPage;
        MapName = mapName;
        ServerName = serverName;

        ArrayListTexts = new List<TextMeshProUGUI>();
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.serverName).GetComponent<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.playerCount).GetComponent<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.mapName).GetComponent<TextMeshProUGUI>());
        //ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.ping).GetComponent<TextMeshProUGUI>());

        TextBackground = GetComponent<Image>();

        SetText(serverName, currentPlayerCount, maxPlayerCount, mapName, ping);
    }
    public void SetText(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping)
    {
        ArrayListTexts[(int)ServerEntryArrayListIndexes.serverName].text = serverName;
        ArrayListTexts[(int)ServerEntryArrayListIndexes.playerCount].text = $"{currentPlayerCount}/{maxPlayerCount}";
        ArrayListTexts[(int)ServerEntryArrayListIndexes.mapName].text = mapName;
        //ArrayListTexts[(int)ServerEntryArrayListIndexes.ping].text = ping.ToString();

        SetArrayList(new ArrayList() { serverName, currentPlayerCount, mapName, ping });
    }
    public void SetBackgroundColor(Color backgroundColor)
    {
        TextBackground.color = backgroundColor;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        ParentServersPage.OnServerEntryHover(this);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        ParentServersPage.OnServerEntrySelect(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ParentServersPage.OnServerEntryExit(this);
    }
}
