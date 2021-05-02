using TMPro;
using UnityEngine;
using UnityEngine.Singleton;

public class PlayerRegistration : MonoBehaviour
{
    public static PlayerRegistration Instance { get => instance; }
    private static PlayerRegistration instance;

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
        TextInputValidator.SanitiseText(ref inputField);
        UsernameInputField.text = inputField;

        IsUsernameValid = TextInputValidator.IsTextLengthValid(inputField);

        PlayerModelUsernameText.text = inputField;
        OKMenuButton.Interactable = IsUsernameValid;
    }
    public void SavePlayerUsername()
    {
        UsernameInputField.text = TextInputValidator.TrimAllSpaces(UsernameInputField.text);

        PlayerPrefs.SetString("Username", UsernameInputField.text);
    }
    public void HideUserRegistration() =>
        gameObject.SetActive(false);

    private void Awake()
    {
        Singleton.Init(ref instance, this);

        UsernameInputField = transform.GetChild(0).GetComponent<TMP_InputField>();
        OKMenuButton = transform.GetChild(1).GetComponent<MenuButton>();

        PlayerModelUsernameText.text = "";
    }
}
