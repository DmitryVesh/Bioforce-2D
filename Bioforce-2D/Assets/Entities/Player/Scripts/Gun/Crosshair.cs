using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance { get; set; }

    private Animator Animator { get; set; }

    
    public void ShotBullet()
    {
        Animator.SetTrigger("shot");
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"Crosshair instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }

        Animator = GetComponent<Animator>();
        GameManager.ShowMouse(false);

        if (GameManager.Instance.IsMobileSupported)
            gameObject.SetActive(false);
    }
    private void Update()
    {
        transform.position = Input.mousePosition;
    }

    private void OnDestroy()
    {
        GameManager.ShowMouse(true);
    }
}
