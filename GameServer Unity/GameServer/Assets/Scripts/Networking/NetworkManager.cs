using GameServer;
using UnityEngine;
using UnityEngine.Singleton;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get => instance; }
    private static NetworkManager instance;

    [SerializeField] private GameObject PlayerPrefab;

    private void Awake()
    {
        Singleton.Init(ref instance, this);
    }

    internal PlayerServer InstantiatePlayer() =>
        Instantiate(PlayerPrefab).GetComponent<PlayerServer>();
}
