using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileJoystick : MonoBehaviour
{
    public static MobileJoystick Instance;

    public void SetActive(bool active)
    {
        if (gameObject != null)
            gameObject.SetActive(active);
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"MobileJoystick instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
        SetActive(false);
    }
    
}
