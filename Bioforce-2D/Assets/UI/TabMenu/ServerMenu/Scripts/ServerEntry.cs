using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ServerEntryArrayListIndexes
{
    //If adding new states, -> 
        //Init(T newItem), 
        //ArrayListTexts.Add(state), 
        //SetText(T newItem), 
        //SetArrayList(T newItem)

    serverName,
    serverState,
    playerCount,
    mapName,
    ping
}
public class ServerEntry : MonoBehaviour, IUIItemListing, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public string MapName { get; private set; }
    public string ServerName { get; private set; }

    private List<TextMeshProUGUI> ArrayListTexts { get; set; }
    [SerializeField] private List<Image> TextBackgrounds;
    private ArrayList ItemList { get; set; }
    private ServersPage ParentServersPage { get; set; }

    public void Init(ServersPage parentPage, string serverName, GameState gameState, int currentPlayerCount, int maxPlayerCount, string mapName, int ping)
    {
        ParentServersPage = parentPage;
        MapName = mapName;
        ServerName = serverName;

        ArrayListTexts = new List<TextMeshProUGUI>();
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.serverName).GetComponentInChildren<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.serverState).GetComponentInChildren<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.playerCount).GetComponentInChildren<TextMeshProUGUI>());
        ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.mapName).GetComponentInChildren<TextMeshProUGUI>());
        //ArrayListTexts.Add(transform.GetChild((int)ServerEntryArrayListIndexes.ping).GetComponentInChildren<TextMeshProUGUI>());

        SetText(serverName, gameState, currentPlayerCount, maxPlayerCount, mapName, ping);
    }

    public void SetText(string serverName, GameState gameState, int currentPlayerCount, int maxPlayerCount, string mapName, int ping)
    {
        ArrayListTexts[(int)ServerEntryArrayListIndexes.serverName].text = serverName;
        ArrayListTexts[(int)ServerEntryArrayListIndexes.serverState].text = gameState.ToString(); //GameStateManager.ServerStates[gameState]; //Use enum translation
        ArrayListTexts[(int)ServerEntryArrayListIndexes.playerCount].text = $"{currentPlayerCount}/{maxPlayerCount}";
        ArrayListTexts[(int)ServerEntryArrayListIndexes.mapName].text = mapName;
        //ArrayListTexts[(int)ServerEntryArrayListIndexes.ping].text = ping.ToString();

        SetArrayList(new ArrayList() { serverName, gameState, currentPlayerCount, mapName, ping });
    }
    public void SetBackgroundColor(Color backgroundColor)
    {
        foreach (Image background in TextBackgrounds)
        {
            background.color = backgroundColor;
        }
        //TextBackground.color = backgroundColor;
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
