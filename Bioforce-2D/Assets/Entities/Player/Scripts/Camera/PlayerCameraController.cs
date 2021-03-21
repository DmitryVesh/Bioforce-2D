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

    ILocalPlayerGun PlayerGun { get; set; }
    private bool OnMobile { get; set; }
    private Transform PlayerTransform { get; set; }

    [SerializeField] private Transform AimFollow;
    [SerializeField] private float MaxX = 3f;
    [SerializeField] private float MaxY = 2f;

    [SerializeField] protected CinemachineImpulseSource ShootImpulse;
    [SerializeField] protected CinemachineImpulseSource HitImpulse;
    [SerializeField] protected CinemachineImpulseSource JumpImpulse;

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

        PlayerManager.OnPlayerShot += GenerateShootImpulse;
        PlayerManager.OnPlayerJumped += GenerateJumpedImpulse;
        PlayerManager.OnPlayerTookDamage += GenerateHitImpulse;

        PlayerGun = GetComponent<ILocalPlayerGun>();
        OnMobile = PlayerGun != null;
        PlayerTransform = PlayerManager.PlayerModelObject.transform;

        AimFollow.parent = null; //Detach object
    }

    private void GenerateHitImpulse(int damage, int currentHealth) =>
        HitImpulse.GenerateImpulse();

    private void GenerateJumpedImpulse() =>
        JumpImpulse.GenerateImpulse();

    private void GenerateShootImpulse(Vector2 position, Quaternion rotation) =>
        ShootImpulse.GenerateImpulse();

    private void Update()
    {
        Vector2 aimingVector = PlayerGun.GetAimingVector();
        AimFollow.position = new Vector2(PlayerTransform.position.x + (aimingVector.x * MaxX),
            PlayerTransform.position.y + (aimingVector.y * MaxY));
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
