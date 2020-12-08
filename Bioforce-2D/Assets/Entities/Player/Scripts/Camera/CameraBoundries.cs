using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoundries : MonoBehaviour
{
    public static CameraBoundries Instance { get; set; }
    public PolygonCollider2D CameraCollider { get; set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"{GetType().Name} instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }

        CameraCollider = GetComponent<PolygonCollider2D>();
    }
}
