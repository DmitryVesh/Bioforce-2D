using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleGDPRConsent;
using System;

public class PrivacyPolicyTermsConditionsMenu : MonoBehaviour
{
    public static Action Accepted { get; set; }

    string PolicyKey = "Policy";
    void Start()
    {
        bool acceptedBefore = PlayerPrefs.GetInt(PolicyKey, 0) == 1;
        if (acceptedBefore)
        {
            Accepted?.Invoke();
            return;
        }

        SimpleGDPR.ShowDialog(new TermsOfServiceDialog().
            SetPrivacyPolicyLink("https://helltraversers.wordpress.com/privacy-policy/").
            SetTermsOfServiceLink("https://helltraversers.wordpress.com/terms-conditions/"),
            OnMenuClosed);
    }

    private void OnMenuClosed()
    {
        PlayerPrefs.SetInt(PolicyKey, 1);
        Accepted?.Invoke();
    }
}
