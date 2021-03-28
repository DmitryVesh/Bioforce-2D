using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VersionCompatibility : MonoBehaviour
{
    public static VersionCompatibility Instance { get; private set; }
    [SerializeField] private GameObject Panel;
    private string GameVersionCurrent { get; set; }
    private string GameVersionLatest { get; set; }

    [SerializeField] private TextMeshProUGUI TextToPlayer;

    public bool DoGameVersionsMatch(string gameVersionLatest) 
    {
        GameVersionLatest = gameVersionLatest;

        Debug.Log($"Latest Version available: {GameVersionLatest}" +
            $"\nCurrent Version Installed: {GameVersionCurrent}");

        return GameVersionCurrent == gameVersionLatest;
    }
    internal void DisplayPanel()
    {
        Panel.SetActive(true);

        TextToPlayer.text = TextToPlayer.text.Replace("x", GameVersionLatest).Replace("z", GameVersionCurrent);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"VersionCompatibility instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }

        Panel.SetActive(false);
        GameVersionCurrent = Application.version;
    }

    
}
