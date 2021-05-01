using UnityEngine;
using UnityEngine.Singleton;
using UnityEngine.UI;

public class ShootingJoystick : MonoBehaviour
{
    public static ShootingJoystick Instance { get => instance; }
    private static ShootingJoystick instance;

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
