using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;
using UnityEngine.UI;

public class MobileJoystick : MonoBehaviour
{
    public static MobileJoystick Instance { get => instance; }
    private static MobileJoystick instance;

    private bool Destroyed;
    [SerializeField] private Image HandleImage;

    public void SetPlayerColor(Color playerColor) =>
        HandleImage.color = playerColor;
    public void SetActive(bool active)
    {
        if (!Destroyed)
            gameObject.SetActive(active);
    }
    private void Awake()
    {
        Singleton.Init(ref instance, this);

        Destroyed = false;
        SetActive(false);
    }
    private void OnDestroy()
    {
        Destroyed = true;
    }
    
}
