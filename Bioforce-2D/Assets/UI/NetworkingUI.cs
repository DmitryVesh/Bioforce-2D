using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkingUI : MonoBehaviour
{
    public static NetworkingUI Instance { get; private set; }
    [SerializeField] private char[] InvalidChars = new char[] { 'q', 'z', 'Z', ']','[', '\'','\"', '@', '+', 'J', ';', '(', ')', '\\', '/', '#', '*', '%','$','£','!','^','&','=','~','?','<','>','|'  };

    private GameObject NetworkMenu { get; set; }
    private TMP_InputField UsernameInputField { get; set; }
    private Button ConnectBtn { get; set; }
    private TMP_Dropdown IPAddressDropDown { get; set; }
    private GameObject ErrorMessagePanel { get; set; }
    private TextMeshProUGUI ErrorMessageTextMesh { get; set; }
    private TouchScreenKeyboard MobileUsernameInputKeyboard { get; set; } = null;
    private TMP_InputField IPAddressManualInputField { get; set; }
    private bool OverrideIP { get; set; } = false;

    public void ConnectToServer()
    {        
        if (OverrideIP)
        {
            string overrideIPAddress = IPAddressManualInputField.text;
            if (Client.IsIPAddressValid(overrideIPAddress))
                Client.Instance.SetManualIPAddressConnectTo(overrideIPAddress);
            else
            {
                DisplayInvalidIPAddressEntered();
                return;
            }
        }

        NetworkMenu.SetActive(false);
        UsernameInputField.interactable = false;
        StartCoroutine(Client.Instance.ConnectToServer());
    }
    public string GetUsername()
    {
        return UsernameInputField.text;
    }
    public void OnUsernameInputFieldChanged()
    {
        if (IsInputFieldValid())
            ConnectBtn.interactable = true;
        else
        {
            //TODO: Shake usernameInputField, make borders red
            ConnectBtn.interactable = false;
        }
    }
    public void OnUsernameInputFieldSelect()
    {
        if (MobileUsernameInputKeyboard != null)
        {
            MobileUsernameInputKeyboard.active = true;
        }
    }
    public void OnUsernameInputFieldDeselect()
    {
        if (MobileUsernameInputKeyboard != null)
        {
            MobileUsernameInputKeyboard.active = false;
        }
    }
    public void OnIPAddressDropDownChange()
    {
        int index = IPAddressDropDown.value;
        Client.Instance.ChangeIPAddressConnectTo(index);

        if (index == 2) //Chosen Manual ip address
        {
            IPAddressManualInputField.gameObject.SetActive(true); //Display manual ip address input field
            OverrideIP = true;
        }
        else
        {
            OverrideIP = false;
            IPAddressManualInputField.gameObject.SetActive(false);
        }
            
    }
    public void Disconnected()
    {
        NetworkMenu.SetActive(true);
        UsernameInputField.interactable = true;
    }

    public void DisplayNoLANServerFound()
    {
        NetworkMenu.SetActive(true);
        ErrorMessageTextMesh.text = "No Server on LAN Connection Found\nTry running a server\nOr connection via Manual IP Setting:\n-UDP Broadcasts might be blocked";
        ActivateErrorMessagePanel();
    }
    public void DisplayTimeOutMessage()
    {
        NetworkMenu.SetActive(true);
        ErrorMessageTextMesh.text = "Your connection has timed out...\n\nTry reconnecting using a different\nIP connection setting";
        ActivateErrorMessagePanel();
    }
    public void DisplayInvalidIPAddressEntered()
    {
        ErrorMessageTextMesh.text = "The IP address entered is invalid\n\nCheck the entry and\nTry again";
        ActivateErrorMessagePanel();
    }
    public void OnErrorMessageContinueBtnPress()
    {
        ErrorMessagePanel.SetActive(false);

        SetInteractableConnectionMenu(true);
    }

    private void ActivateErrorMessagePanel()
    {
        ErrorMessagePanel.SetActive(true);
        SetInteractableConnectionMenu(false);
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"SimpleNetworkingUI instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }

        NetworkMenu = transform.GetChild(0).gameObject;
        NetworkMenu.SetActive(true);
        UsernameInputField = NetworkMenu.transform.GetChild(0).GetComponent<TMP_InputField>();
        ConnectBtn = NetworkMenu.transform.GetChild(1).GetComponent<Button>();
        IPAddressDropDown = NetworkMenu.transform.GetChild(2).GetComponent<TMP_Dropdown>();
        IPAddressManualInputField = IPAddressDropDown.gameObject.GetComponentInChildren<TMP_InputField>();
        IPAddressManualInputField.gameObject.SetActive(false);
        ErrorMessagePanel = NetworkMenu.transform.GetChild(3).gameObject;
        ErrorMessageTextMesh = ErrorMessagePanel.GetComponentInChildren<TextMeshProUGUI>();
        ErrorMessagePanel.SetActive(false);

        ConnectBtn.interactable = false;

        if (TouchScreenKeyboard.isSupported)
        {
            MobileUsernameInputKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, true);
            TouchScreenKeyboard.hideInput = true;
            MobileUsernameInputKeyboard.active = false;
        }
    } 
    private void SetInteractableConnectionMenu(bool active)
    {
        ConnectBtn.interactable = active;
        UsernameInputField.interactable = active;
        IPAddressDropDown.interactable = active;
    }
    private bool IsInputFieldValid()
    {
        string inputField = UsernameInputField.text;
        RemoveInvalidChars(ref inputField);
        UsernameInputField.text = inputField;
        if (inputField == null || inputField == "" || inputField.Length < 3)
            return false;
        return true;
    }
    private void RemoveInvalidChars(ref string String)
    {
        List<int> indexesRemove = new List<int>();

        for (int count = 0; count < String.Length; count++)
        {
            foreach (char invalidChar in InvalidChars)
            {
                if (String[count] == invalidChar)
                {
                    indexesRemove.Add(count);
                    break;
                }

            }
        }

        int decrimentIndexer = 0;
        foreach (int index in indexesRemove)
        {
            String = String.Remove(index - decrimentIndexer);
            decrimentIndexer++;
        }
    }
}
