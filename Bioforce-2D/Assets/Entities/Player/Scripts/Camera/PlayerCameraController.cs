using Cinemachine;
using System;
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
    private Transform PlayerModel;
    private float MaxX { get; set; } = 4f;
    private float MaxY { get; set; } = 2f;
    bool FacingRight { get; set; } = true;
    bool LastFacingRight { get; set; } = true;
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

        MobileLocalPlayerGun = GetComponent<MobileLocalPlayerGun>();
        OnMobile = MobileLocalPlayerGun != null;
        PlayerModel = PlayerManager.PlayerModelObject.transform;
        Follow.position = PlayerModel.position;
        //OnMobile = false;

        if (!OnMobile)
        {
            Follow = PlayerManager.PlayerModelObject.transform;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (!OnMobile)
            return;


        //WorldPosition();
        //LocalPosition();
    }

    private float damping = 1.25f;
    private void WorldPosition()
    {
        //Better
        Vector3 newPosition = new Vector2(PlayerModel.position.x + (MobileLocalPlayerGun.LastX * MaxX),
            PlayerModel.position.y + (MobileLocalPlayerGun.LastY * MaxY));
        //CameraTransposer.OnTargetObjectWarped(CameraVirtual.Follow, newPosition - Follow.position);
        //Follow.position = newPosition;
        Follow.position = Vector3.Lerp(Follow.position, newPosition, damping * Time.fixedDeltaTime);
    }
    private void LocalPosition()
    {
        float TimesMinus = 1;
        float y = Follow.rotation.eulerAngles.y;
        if (y >= -190 && y <= -170 || y >= 170 && y <= 190)
        {
            TimesMinus = -1;
            FacingRight = false;
        }
        else
            FacingRight = true;


        Vector3 newPosition = new Vector2(MobileLocalPlayerGun.LastX * MaxX * TimesMinus, MobileLocalPlayerGun.LastY * MaxY);
        Follow.localPosition = newPosition;

        if (LastFacingRight != FacingRight)
        {
            //CameraTransposer.OnTargetObjectWarped(CameraVirtual.Follow, newPosition - CameraVirtual.Follow.position);
            CameraTransposer.OnTargetObjectWarped(CameraVirtual.Follow, newPosition);
            Debug.Log("Called this");
        }
        LastFacingRight = FacingRight;
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
