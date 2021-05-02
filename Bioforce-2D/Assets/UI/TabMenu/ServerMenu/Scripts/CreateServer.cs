using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Output;

public class CreateServer : MonoBehaviour
{
    //Server name
    [SerializeField] TMP_InputField ServerNameInputField;

    //Map selection
    [SerializeField] TMP_Dropdown MapDropdown;
    [SerializeField] private List<string> MapNames 
        = new List<string> { "Level 1" };


    //Player count
    [SerializeField] Slider MaximumPlayerSlider;
    [SerializeField] TextMeshProUGUI MaxPlayersText;

    //Public/Private server
    [SerializeField] Text PublicPrivateText;
    [SerializeField] Image PublicPrivateBackground;
    [SerializeField] Color PublicBackground;
    [SerializeField] Color PrivateBackground;
    private bool ClickedOddPublic { get; set; }

    //Internet/LAN server
    [SerializeField] Text InternetLANText;
    [SerializeField] Image InternetLANBackground;
    private bool ClickedOddInternet { get; set; }


    //Start Server
    [SerializeField] MenuButton StartServerButton;
    [SerializeField] GameObject ServerGameObject;

    //Final Server Data
    [HideInInspector] public string ServerNameSelected;
    public string MapSelected { get; private set; }
    public int MaxPlayerSelected { get; private set; }
    public bool PublicServerSelected { get; private set; } = true;
    public bool InternetServerSelected { get; private set; } = true;


    //Events subscribed by Inspector
    public void OnServerNameChanged()
    {
        ServerNameSelected = ServerNameInputField.text;
        TextInputValidator.SanitiseText(ref ServerNameSelected, TextType.File);
        ServerNameInputField.text = ServerNameSelected;
        StartServerButton.Interactable = TextInputValidator.IsTextLengthValid(ServerNameSelected);
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
        ClickedOddPublic = !ClickedOddPublic;
        if (!ClickedOddPublic)
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
    public void OnInternetLANGameToggle()
    {
        ClickedOddInternet = !ClickedOddInternet;
        if (!ClickedOddInternet)
            return;

        InternetServerSelected = !InternetServerSelected;
        string toggleText;
        Color toggleBackgroundColor;
        if (InternetServerSelected)
        {
            toggleBackgroundColor = PublicBackground;
            toggleText = "Internet Server";
        }
        else
        {
            toggleBackgroundColor = PrivateBackground;
            toggleText = "LAN Server";
        }
        InternetLANText.text = toggleText;
        InternetLANBackground.color = toggleBackgroundColor;
    }
    public void OnStartServer()
    {
        bool successStart;
        if (!InternetServerSelected)
        {
            throw new NotImplementedException();
            //int port = 28025; //TODO: Make so not hardcoded, maybe player assigns their own?
            //successStart = ServerProgram.StartServerProgram(ServerNameSelected, MaxPlayerSelected, MapSelected, port);
            //Instantiate(ServerGameObject);
            //Client.Instance.ConnectToServer("127.0.0.1", port);
        }
        else 
        {
            ServerMenu.Instance.HideServerMenu();
            ServerNameSelected = TextInputValidator.TrimAllSpaces(ServerNameSelected);
            successStart = InternetServerScanner.Instance.ContactMainServerToAddOwnServer(ServerNameSelected, MaxPlayerSelected, MapSelected, Client.PortNumInternetToConnectTo);
            //Waiting for response packet from MainServer
        }
        //TODO: Implement the Public/Private servers

        if (!successStart)
        {
            Output.WriteLine($"Error, {(InternetServerSelected ? "Internet" : "LAN" )} server not started...");
            return;
        }
    }

    void Start()
    {
        SetMapDropdownOptions();
        OnMaxPlayerSliderChanged();
        StartServerButton.Interactable = false;
    }


    
    private void SetMapDropdownOptions()
    {
        MapDropdown.ClearOptions();
        MapDropdown.AddOptions(MapNames);
        MapSelected = MapNames[0];
    }

}
