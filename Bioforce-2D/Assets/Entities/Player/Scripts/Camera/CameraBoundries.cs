using UnityEngine;
using UnityEngine.Singleton;

public class CameraBoundries : MonoBehaviour
{
    public static CameraBoundries Instance { get => instance; }
    private static CameraBoundries instance;

    public PolygonCollider2D CameraCollider { get; set; }
    
    private void Awake()
    {
        Singleton.Init(ref instance, this);

        CameraCollider = GetComponent<PolygonCollider2D>();
    }
}
