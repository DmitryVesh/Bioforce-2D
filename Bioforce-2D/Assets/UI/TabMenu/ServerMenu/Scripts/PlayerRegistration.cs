using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRegistration : MonoBehaviour
{
    public static PlayerRegistration Instance;

    public bool IsUsernameValid { get; set; }

    [SerializeField] private TextMeshProUGUI PlayerModelUsernameText;
    [SerializeField] private char[] InvalidChars = new char[] { 'q', 'z', 'Z', ']', '[', '\'', '\"', '@', '+', 'J', ';', '(', ')', '\\', '/', '#', '*', '%', '$', '£', '!', '^', '&', '=', '~', '?', '<', '>', '|' };

    private TMP_InputField UsernameInputField { get; set; }
    private MenuButton OKMenuButton { get; set; }

    public void DisplayUserRegistration()
    {
        if (PlayerPrefs.HasKey("Username"))
        {
            string username = PlayerPrefs.GetString("Username");
            UsernameInputField.text = username;
            PlayerModelUsernameText.text = username;
        }
        else
        {
            OKMenuButton.Interactable = false;
            PlayerModelUsernameText.text = "";
        }
    }
    public void OnUsernameInputFieldChanged() //Called by inputField on change event
    {
        string inputField = UsernameInputField.text;
        RemoveInvalidCharsInString(ref inputField);
        UsernameInputField.text = inputField;

        IsUsernameValid = IsInputFieldValid(inputField);

        PlayerModelUsernameText.text = inputField;
        OKMenuButton.Interactable = IsUsernameValid;
    }
    //TODO: Test on android, on my iOS device when click on inputField, it auto opens a keyboard for me...
    //So might be useless.
    /*
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
    */
    public void SavePlayerUsername() =>
        PlayerPrefs.SetString("Username", UsernameInputField.text);
    public void HideUserRegistration() =>
        gameObject.SetActive(false);

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"PlayerRegistration instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
        UsernameInputField = transform.GetChild(0).GetComponent<TMP_InputField>();
        OKMenuButton = transform.GetChild(1).GetComponent<MenuButton>();

        PlayerModelUsernameText.text = "";
    }

    private bool IsInputFieldValid(string inputField)
    {
        if (inputField == null || inputField == "" || inputField.Length < 3)
            return false;
        return true;
    }
    private void RemoveInvalidCharsInString(ref string String)
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
