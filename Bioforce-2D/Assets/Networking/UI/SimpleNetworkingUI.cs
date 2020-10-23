using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleNetworkingUI : MonoBehaviour
{
    public static SimpleNetworkingUI instance { get; private set; }

    private GameObject NetworkMenu { get; set; }
    private TMP_InputField UsernameInputField { get; set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log($"SimpleNetworkingUI instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
        NetworkMenu = transform.GetChild(0).gameObject;
        UsernameInputField = NetworkMenu.transform.GetChild(0).GetComponent<TMP_InputField>();
    }

    public void ConnectToServer()
    {
        NetworkMenu.SetActive(false);
        UsernameInputField.interactable = false;

        Client.instance.ConnectToServer();
    }
}
