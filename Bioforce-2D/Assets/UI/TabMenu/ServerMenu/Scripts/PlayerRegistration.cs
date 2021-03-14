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

    private TMP_InputField UsernameInputField { get; set; }
    private MenuButton OKMenuButton { get; set; }

    public static string GetUsername() =>
        PlayerPrefs.GetString("Username");

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
        TextInputValidator.RemoveInvalidCharsInString(ref inputField);
        UsernameInputField.text = inputField;

        IsUsernameValid = TextInputValidator.IsTextLengthValid(inputField);

        PlayerModelUsernameText.text = inputField;
        OKMenuButton.Interactable = IsUsernameValid;
    }
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
            Destroy(gameObject);
        }
        UsernameInputField = transform.GetChild(0).GetComponent<TMP_InputField>();
        OKMenuButton = transform.GetChild(1).GetComponent<MenuButton>();

        PlayerModelUsernameText.text = "";
    }
}
