using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance { get => instance; }
    private static Crosshair instance;

    private Animator Animator { get; set; }

    
    public void ShotBullet()
    {
        Animator.SetTrigger("shot");
    }
    private void Awake()
    {
        Singleton.Init(ref instance, this);

        Animator = GetComponent<Animator>();           
    }
    private void Start()
    {
        if (GameManager.IsMobileSupported)
            gameObject.SetActive(false);

        GameManager.ShowMouse(false);
        
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
