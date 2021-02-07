using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameServer;
using System.Net;

public class CreateServer : MonoBehaviour
{
    //Server name
    [SerializeField] TMP_InputField ServerNameInputField;
    //TODO: Add swear words and invalid server names for server creation
    [SerializeField] private List<string> InvalidServerNameStrings
        = new List<string>() { "shit" };

    //Map selection
    [SerializeField] TMP_Dropdown MapDropdown;
    [SerializeField] private List<string> MapNames 
        = new List<string> { "Level 1" };


    //Player count
    [SerializeField] Slider MaximumPlayerSlider;
    [SerializeField] TextMeshProUGUI MaxPlayersText;

    //Public/Private server
    [SerializeField] Toggle PublicPrivateGameToggle;
    [SerializeField] Text PublicPrivateText;
    [SerializeField] Image PublicPrivateBackground;
    [SerializeField] Color PublicBackground;
    [SerializeField] Color PrivateBackground;
    private bool ClickedOdd { get; set; }


    //Start Server
    [SerializeField] Button StartServerButton;
    [SerializeField] GameObject ServerGameObject;

    //Final Server Data
    public string ServerNameSelected { get; private set; }
    public string MapSelected { get; private set; }
    public int MaxPlayerSelected { get; private set; }
    public bool PublicServerSelected { get; private set; } = true;


    //Events subscribed by Inspector
    public void OnServerNameChanged()
    {
        ServerNameSelected = ServerNameInputField.text;
        ServerNameSelected = ServerNameSelected.Replace(" ", null);
        ServerNameInputField.text = ServerNameSelected;
        StartServerButton.interactable = IsServerNameValid(ServerNameSelected);
    }
    public void OnMapDropdownChanged()
    {
        MapSelected = MapNames[MapDropdown.value];
    }
    public void OnMaxPlayerSliderChanged()
    {
        MaxPlayerSelected = (int)MaximumPlayerSlider.value;
        MaxPlayersText.text = MaxPlayerSelected.ToString();
    }
    public void OnPublicPrivateGameToggle()
    {
        ClickedOdd = !ClickedOdd;
        if (!ClickedOdd)
            return;

        PublicServerSelected = !PublicServerSelected;
        string toggleText;
        Color toggleBackgroundColor;
        if (PublicServerSelected)
        {
            toggleBackgroundColor = PublicBackground;
            toggleText = "Public Server";
        }
        else
        {
            toggleBackgroundColor = PrivateBackground;
            toggleText = "Private Server";
        }
        PublicPrivateText.text = toggleText;
        PublicPrivateBackground.color = toggleBackgroundColor;
    }
    public void OnStartServer()
    {
        //TODO: Implement the Public/Private servers
        bool startedSuccessfully = ServerProgram.StartServerProgram(ServerNameSelected, MaxPlayerSelected, MapSelected);        

        if (!startedSuccessfully)
            return;

        Instantiate(ServerGameObject);
        Client.Instance.ConnectToServer(IPAddress.Loopback.ToString());
    }

    void Start()
    {
        SetMapDropdownOptions();
        OnMaxPlayerSliderChanged();
        StartServerButton.interactable = false;
    }

    private bool IsServerNameValid(string serverName) =>
        serverName.Length >= 5 && !InvalidServerNameStrings.Any(serverName.ToLower().Contains);
    private void SetMapDropdownOptions()
    {
        MapDropdown.ClearOptions();
        MapDropdown.AddOptions(MapNames);
        MapSelected = MapNames[0];
    }

}
