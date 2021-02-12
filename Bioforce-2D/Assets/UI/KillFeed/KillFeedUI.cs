using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedUI : UIEntryManager
{
    public static KillFeedUI Instance { get; private set; }

    [SerializeField] private Color BorderColor = new Color(212, 0, 0);
    [SerializeField] private Color NormalFillColor = new Color(0, 0, 0);

    public void AddKillFeedEntry(int playerDiedID, int bulletOwnerID)
    {
        string playerDiedName = GameManager.PlayerDictionary[playerDiedID].GetUsername();
        string killerPlayerName = GameManager.PlayerDictionary[bulletOwnerID].GetUsername();

        GameObject killFeedEntry = AddEntry($"{killerPlayerName} killed {playerDiedName}");

        int clientInstanceID = Client.Instance.ClientID;
        if (clientInstanceID == playerDiedID)
        {
            TurnOnRedBorder(killFeedEntry);
            TurnOnRedFilling(killFeedEntry);
        }
        else if (clientInstanceID == bulletOwnerID)
        {
            TurnOnRedBorder(killFeedEntry);
            TurnOnNormalFilling(killFeedEntry);
        }
        else
        {
            TurnOffBorder(killFeedEntry);
            TurnOnNormalFilling(killFeedEntry);
        }
    }

    protected override void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"KillFeedUI instance already exists, destorying {gameObject.name}");
            Destroy(gameObject);
        }
        base.Awake();
    }

    
    private void TurnOffBorder(GameObject entry)
    {
        entry.transform.GetChild(1).gameObject.SetActive(false);
    }
    private void TurnOnRedBorder(GameObject entry)
    {
        entry.transform.GetChild(1).gameObject.SetActive(true);
    }
    private void TurnOnRedFilling(GameObject entry)
    {
        entry.GetComponent<Image>().color = BorderColor;
    }
    private void TurnOnNormalFilling(GameObject entry)
    {
        entry.GetComponent<Image>().color = NormalFillColor;
    }
    
    
    
    
}
