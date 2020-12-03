using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedUI : MonoBehaviour
{
    public static KillFeedUI Instance { get; private set; }

    [SerializeField] private float KillFeedEntryTimeToLive = 8;
    private GameObject KillFeedPanel { get; set; }
    private List<GameObject> KillFeedEntryPanels { get; set; }
    private List<TextMeshProUGUI> KillFeedEntryTexts { get; set; }

    private Queue<GameObject> ActiveKillFeedEntries { get; set; }

    [SerializeField] private Color BorderColor = new Color(212, 0, 0);
    [SerializeField] private Color NormalFillColor = new Color(0, 0, 0);

    public void AddKillFeedEntry(int playerDiedID, int bulletOwnerID)
    {
        string playerDiedName = GameManager.PlayerDictionary[playerDiedID].GetUsername();
        string killerPlayerName = GameManager.PlayerDictionary[bulletOwnerID].GetUsername();

        int inactiveEntryIndex = FindIndexOfInactiveEntry();
        GameObject killFeedEntry;


        if (inactiveEntryIndex == -1)
            killFeedEntry = GetLongestActiveEntry();
        else
            killFeedEntry = KillFeedEntryPanels[inactiveEntryIndex];


        killFeedEntry.SetActive(true);
        KillFeedEntryTexts[inactiveEntryIndex].text = $"{killerPlayerName} killed {playerDiedName}";
        ActiveKillFeedEntries.Enqueue(killFeedEntry);

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

        StartCoroutine(RemoveKillFeedEntry(killFeedEntry));
        return;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"KillFeedUI instance already exists, destorying {gameObject.name}");
            Destroy(this);
        }

        KillFeedPanel = transform.GetChild(0).gameObject;

        InitKillFeedEntryPanels();
        InitKillFeedEntryTexts(); 
    }
    
    private int FindIndexOfInactiveEntry()
    {
        for (int count = 0; count < KillFeedEntryPanels.Count; count++)
        {
            GameObject entry = KillFeedEntryPanels[count];
            if (!entry.activeInHierarchy)
                return count;
        }
        return -1;
    }
    private GameObject GetLongestActiveEntry()
    {
        GameObject entry = ActiveKillFeedEntries.Dequeue();
        entry.SetActive(false);
        return entry;
    }

    private IEnumerator RemoveKillFeedEntry(GameObject killFeedEntry)
    {
        yield return new WaitForSeconds(KillFeedEntryTimeToLive);
        //TODO: make disappering decreasing in alpha, then deactivating
        killFeedEntry.SetActive(false);
        ActiveKillFeedEntries.Dequeue();
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
    private void InitKillFeedEntryPanels()
    {
        Transform killFeedPanelTransform = KillFeedPanel.transform;
        int numKillFeedPanels = killFeedPanelTransform.childCount;
        KillFeedEntryPanels = new List<GameObject>(numKillFeedPanels);
        for (int count = 0; count < numKillFeedPanels; count++)
        {
            KillFeedEntryPanels.Add(killFeedPanelTransform.GetChild(count).gameObject);
            KillFeedEntryPanels[count].SetActive(false);
        }
            
    }
    private void InitKillFeedEntryTexts()
    {
        KillFeedEntryTexts = new List<TextMeshProUGUI>(KillFeedEntryPanels.Count);
        foreach (GameObject killFeedEntry in KillFeedEntryPanels)
            KillFeedEntryTexts.Add(killFeedEntry.GetComponentInChildren<TextMeshProUGUI>());
    }
    
    
}
