using Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private GameObject CinemachineCamera;
    private CinemachineConfiner CameraConfiner { get; set; }
    private CinemachineFramingTransposer CameraTransposer { get; set; }
    private CinemachineVirtualCamera CameraVirtual { get; set; }

    private PlayerManager PlayerManager { get; set; }

    private void Awake()
    {
        CameraConfiner = CinemachineCamera.GetComponent<CinemachineConfiner>();
        CameraConfiner.m_BoundingShape2D = CameraBoundries.Instance.CameraCollider;
                        
        CameraVirtual = CinemachineCamera.GetComponent<CinemachineVirtualCamera>();
        CameraTransposer = CameraVirtual.GetCinemachineComponent<CinemachineFramingTransposer>();
    }
    private void Start()
    {
        PlayerManager = GameManager.PlayerDictionary[Client.Instance.ClientID];
        PlayerManager.OnPlayerRespawn += ResetCamera;

    }

    private void ResetCamera()
    {
        CameraTransposer.OnTargetObjectWarped(CameraVirtual.Follow, RespawnPoint.Instance.LastRespawnPoint - RespawnPoint.Instance.LastDiePosition);
    }

    private void OnDestroy()
    {
        PlayerManager.OnPlayerRespawn -= ResetCamera;
        Destroy(CinemachineCamera);
    }
}
