using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleNetworkingUI : MonoBehaviour
{
    public static SimpleNetworkingUI Instance { get; private set; }

    private GameObject NetworkMenu { get; set; }
    private TMP_InputField UsernameInputField { get; set; }
    private Button ConnectBtn { get; set; }
    private TMP_Dropdown IPAddressDropDown { get; set; }
    private GameObject TimeOutPanel { get; set; }


    public void ConnectToServer()
    {
        NetworkMenu.SetActive(false);
        UsernameInputField.interactable = false;

        Client.Instance.ConnectToServer();
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
    public void OnIPAddressDropDownChange()
    {
        int index = IPAddressDropDown.value;
        Client.Instance.ChangeIPAddressConnectTo(index);
    }
    public void Disconnected()
    {
        NetworkMenu.SetActive(true);
        UsernameInputField.interactable = true;
    }
    public void DisplayTimeOutMessage()
    {
        TimeOutPanel.SetActive(true);

        SetInteractableConnectionMenu(false);
    }
    public void OnTimeOutContinueBtnPress()
    {
        TimeOutPanel.SetActive(false);

        SetInteractableConnectionMenu(true);
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
        UsernameInputField = NetworkMenu.transform.GetChild(0).GetComponent<TMP_InputField>();
        ConnectBtn = NetworkMenu.transform.GetChild(1).GetComponent<Button>();
        IPAddressDropDown = NetworkMenu.transform.GetChild(2).GetComponent<TMP_Dropdown>();

        TimeOutPanel = NetworkMenu.transform.GetChild(3).gameObject;
        TimeOutPanel.SetActive(false);

        ConnectBtn.interactable = false;
    } 
    private void SetInteractableConnectionMenu(bool active)
    {
        ConnectBtn.interactable = active;
        UsernameInputField.interactable = active;
        IPAddressDropDown.interactable = active;
    }
    //TODO: Add that min length should be like 4 characters, and that it can't be bad words
    private bool IsInputFieldValid()
    {
        string inputField = UsernameInputField.text;
        if (inputField == null || inputField == "")
            return false;
        return true;
    }
}
