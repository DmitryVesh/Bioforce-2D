using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManualServerEntry : MonoBehaviour
{
    [SerializeField] private TMP_InputField IPInputField;

    public string GetIPAddress() =>
        IPInputField.text;
    public void OnIPInputFieldChanged()
    {
        string ipAddress = IPInputField.text;
        ServerMenu.SetManualIPAddress(Client.IsIPAddressValid(ipAddress));
    }
}
