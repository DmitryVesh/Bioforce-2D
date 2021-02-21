using Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private GameObject CinemachineCamera;
    private CinemachineConfiner CameraConfiner { get; set; }
    private CinemachineFramingTransposer CameraTransposer { get; set; }
    private CinemachineVirtualCamera CameraVirtual { get; set; }

    private PlayerManager PlayerManager { get; set; }

    MobileLocalPlayerGun MobileLocalPlayerGun { get; set; }
    private bool OnMobile { get; set; }
    [SerializeField] private Transform Follow;
    private float MaxX = 4f;
    private float MaxY = 2f;

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

        //MobileLocalPlayerGun = GetComponent<MobileLocalPlayerGun>();
        //OnMobile = MobileLocalPlayerGun != null;
        OnMobile = false;

        if (!OnMobile)
        {
            Follow = PlayerManager.PlayerModelObject.transform;
            return;
        }
    }
    private void Update()
    {
        if (!OnMobile)
            return;

        float TimesMinus = 1;
        float y = Follow.rotation.eulerAngles.y;
        if (y >= -190 && y <= -170 || y >= 170 && y <= 190)
            TimesMinus = -1;
        Follow.localPosition = new Vector2(MobileLocalPlayerGun.LastX * MaxX * TimesMinus, MobileLocalPlayerGun.LastY * MaxY);
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
