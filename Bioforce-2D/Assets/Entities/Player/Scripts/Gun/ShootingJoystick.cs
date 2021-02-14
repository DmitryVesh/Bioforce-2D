using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingJoystick : MonoBehaviour
{
    public static ShootingJoystick Instance;
    private bool Destroyed;

    public void SetActive(bool active)
    {
        if (!Destroyed)
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
            Debug.Log($"ShootingJoystick instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }
        Destroyed = false;
        SetActive(false);
    }
    private void OnDestroy()
    {
        Destroyed = true;
    }
}
