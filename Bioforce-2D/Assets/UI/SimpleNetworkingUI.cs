using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleNetworkingUI : MonoBehaviour
{
    public static SimpleNetworkingUI Instance { get; private set; }

    private GameObject NetworkMenu { get; set; }
    private TMP_InputField UsernameInputField { get; set; }
    
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

    public void Disconnected()
    {
        NetworkMenu.SetActive(true);
        UsernameInputField.interactable = true;
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
    } 
}
