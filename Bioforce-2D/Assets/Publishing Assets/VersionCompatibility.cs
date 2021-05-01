using TMPro;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;

public class VersionCompatibility : MonoBehaviour
{
    public static VersionCompatibility Instance { get => instance; }
    private static VersionCompatibility instance;

    [SerializeField] private GameObject Panel;
    private string GameVersionCurrent { get; set; }
    private string GameVersionLatest { get; set; }

    [SerializeField] private TextMeshProUGUI TextToPlayer;

    public bool DoGameVersionsMatch(string gameVersionLatest) 
    {
        GameVersionLatest = gameVersionLatest;

        Output.WriteLine($"Latest Version available: {GameVersionLatest}" +
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
        Singleton.Init(ref instance, this);

        Panel.SetActive(false);
        GameVersionCurrent = Application.version;
    }

    
}
