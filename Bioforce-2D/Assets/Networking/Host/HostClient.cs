using System.Collections.Generic;
using UnityEngine;

public class HostClient : MonoBehaviour
{
    public static HostClient Instance { get; private set; }
    private HostStateManager[] HostStateManagers { get; set; }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"HostClient instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        HostStateManagers = GetComponentsInChildren<HostStateManager>(true);
    }

    public void Host(bool shouldHost)
    {
        foreach (HostStateManager hostStateManager in HostStateManagers)
            hostStateManager.Host(shouldHost);
    }


    
}
