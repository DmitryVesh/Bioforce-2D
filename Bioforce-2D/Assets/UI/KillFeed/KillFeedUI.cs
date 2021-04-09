using UnityEngine;
using UnityEngine.UI;

public class KillFeedUI : UIEntryManager
{
    public static KillFeedUI Instance { get; private set; }

    [SerializeField] private Color BorderColor = new Color(212, 0, 0);
    [SerializeField] private Color NormalFillColor = new Color(0, 0, 0);

    [SerializeField] private Sprite DeathBullet;
    [SerializeField] private Sprite DeathFall;

    public void AddKillFeedEntry(byte diedID, byte killerID)
    {
        string killerName = "";
        string diedName = GameManager.PlayerDictionary[diedID].GetUsername();

        Sprite deathSprite;

        Color imageColor;
        Color killerColor = GameManager.PlayerDictionary[killerID].PlayerColor;
        Color diedColor = GameManager.PlayerDictionary[diedID].PlayerColor;
        

        if (diedID == killerID)
        {
            deathSprite = DeathFall;
            imageColor = diedColor;
        }
        else
        {
            deathSprite = DeathBullet;
            killerName = GameManager.PlayerDictionary[killerID].GetUsername();
            imageColor = killerColor;
        }
        GameObject killFeedEntry = AddEntry(killerName, diedName, deathSprite, imageColor, killerColor, diedColor);

        byte clientInstanceID = Client.Instance.ClientID;
        if (clientInstanceID == diedID)
        {
            TurnOnRedBorder(killFeedEntry);
            TurnOnRedFilling(killFeedEntry);
        }
        else if (clientInstanceID == killerID)
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
        entry.transform.GetChild(0).gameObject.SetActive(false);
    }
    private void TurnOnRedBorder(GameObject entry)
    {
        entry.transform.GetChild(0).gameObject.SetActive(true);
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
