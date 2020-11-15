using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private GameObject CinemachineCamera;

    private void OnDestroy()
    {
        Destroy(CinemachineCamera);
    }
}
