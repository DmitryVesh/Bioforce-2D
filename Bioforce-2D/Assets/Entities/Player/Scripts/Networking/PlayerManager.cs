using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int ID;
    [SerializeField] private string Username;

    private GameObject PlayerModelObject { get; set; }
    private TextMeshProUGUI UsernameText { get; set; }

    // Delegate and event used to notify when movement stats are read from server
    public delegate void PlayerMovementStats(float runSpeed);
    public event PlayerMovementStats OnPlayerMovementStatsChanged;

    public delegate void PlayerSpeedX(float speedX);
    public event PlayerSpeedX OnPlayerSpeedXChanged;

    public delegate void IsPlayerAlive();
    public event IsPlayerAlive OnPlayerDeath;
    public event IsPlayerAlive OnPlayerRespawn;
    public static float RespawnTime { get; private set; } = 1.5f;
    public static float DeadTime { get; private set; } = 2;

    private IGun PlayerGun;


    public string GetUsername()
    {
        return Username;
    }
    public void SetPlayerMovementStats(float runSpeed, float sprintSpeed)
    {
        OnPlayerMovementStatsChanged?.Invoke(runSpeed);
    }
    public void SetVelocity(Vector2 velocity)
    {
        OnPlayerSpeedXChanged?.Invoke(velocity.x);
    }
    public void PlayerDied()
    {
        OnPlayerDeath?.Invoke();
    }
    public IEnumerator IsPlayerDeadUponSpawning(bool isDead)
    {
        if (isDead)
        {
            //Prevents calling before the Start methods which subscribe to the event
            yield return new WaitForSeconds(0.3f);
            OnPlayerDeath?.Invoke();
        }
    }
    public void PlayerRespawned()
    {
        //TODO: 3000 Add a check pointing system, which is like deathmatch avaialbe spawn points
        //Make seperate class called PlayerSpawning 
        CallOnPlayerRespawnEvent();        
    }
    public void CallOnPlayerRespawnEvent()
    {
        OnPlayerRespawn?.Invoke();
    }

    public void Initialise(int iD, string username) 
    {
        (ID, Username) = (iD, username);
        PlayerGun = transform.GetComponentInChildren<IGun>();
        PlayerGun.SetOwnerClientID(iD);
        SetUsername();
    }
    public void SetPosition(Vector3 position)
    {
        PlayerModelObject.transform.position = position;
    }
    public void SetRotation(Quaternion rotation)
    {
        PlayerModelObject.transform.rotation = rotation;
    }
    
    public void Disconnect()
    {
        Destroy(gameObject);
    }
    public void ShotBullet(Vector2 position, Quaternion rotation)
    {
        PlayerGun.ShootBullet(position, rotation);
    }

    private void Awake()
    {
        PlayerModelObject = transform.GetChild(0).gameObject;
        UsernameText = PlayerModelObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        SetUsername("");
    }
    
    private void SetUsername()
    {
        UsernameText.text = Username;
    }
    private void SetUsername(string username)
    {
        UsernameText.text = username;
    }

    
}
