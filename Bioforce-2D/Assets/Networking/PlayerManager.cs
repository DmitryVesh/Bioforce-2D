using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] public int ID;
    [SerializeField] private string Username;

    public int Kills { get; private set; }
    public int Deaths { get; private set; }
    public int Score { get; private set; }

    public Vector2 RespawnPosition { get; set; }
    private GameObject PlayerModelObject { get; set; }
    private TextMeshProUGUI UsernameText { get; set; }

    // Delegate and event used to notify when movement stats are read from server
    public delegate void PlayerMovementStats(float runSpeed);
    public event PlayerMovementStats OnPlayerMovementStatsChanged;

    public delegate void PlayerSpeedX(float speedX);
    public event PlayerSpeedX OnPlayerSpeedXChanged;

    public delegate void PlayerRespawn();
    public event PlayerRespawn OnPlayerRespawn;
    public delegate void PlayerDeath(TypeOfDeath typeOfDeath);
    public event PlayerDeath OnPlayerDeath;
    public static float RespawnTime { get; private set; } = 1.5f;
    public static float DeadTime { get; private set; } = 3;

    public delegate void PlayerTookDamage(int damage, int currentHealth);
    public event PlayerTookDamage OnPlayerTookDamage;

    public delegate void PlayerShot(Vector2 position, Quaternion rotation);
    public event PlayerShot OnPlayerShot;
    
    public delegate void PlayerJumped();
    public event PlayerJumped OnPlayerJumped;

    public delegate void PlayerPosition(Vector2 position);
    public event PlayerPosition OnPlayerPosition;
    public delegate void PlayerRotation(Quaternion rotation);
    public event PlayerRotation OnPlayerRotation;

    public event PlayerShot OnArmPositionRotation;


    public string GetUsername() =>
        Username;
    public void SetPlayerMovementStats(float runSpeed, float sprintSpeed) =>
        OnPlayerMovementStatsChanged?.Invoke(runSpeed);

    public void SetVelocity(Vector2 velocity) =>
        OnPlayerSpeedXChanged?.Invoke(velocity.x);

    public void PlayerDied(TypeOfDeath typeOfDeath) =>
        OnPlayerDeath?.Invoke(typeOfDeath);

    public IEnumerator IsPlayerDeadUponSpawning(bool isDead)
    {
        if (isDead)
        {
            //Prevents calling before the Start methods which subscribe to the event
            yield return new WaitForSeconds(0.3f);
            OnPlayerDeath?.Invoke(TypeOfDeath.Bullet);
        }
    }
    public void PlayerRespawned() =>
        OnPlayerRespawn?.Invoke();

    public void CallOnPlayerJumpedEvent() =>
        OnPlayerJumped?.Invoke();

    public void Initialise(int iD, string username) 
    {
        (ID, Username) = (iD, username);
        SetUsername();
    }

    public void SetRespawnPosition(Vector2 position)
    {
        PlayerModelObject.transform.position = position;
        SetPosition(position);
    }
    public void SetPosition(Vector2 position) =>
        OnPlayerPosition?.Invoke(position); 
    public void SetRotation(Quaternion rotation) =>
        OnPlayerRotation?.Invoke(rotation); 

        
    
    public void Disconnect() =>
        Destroy(gameObject);


    public void CallOnBulletShotEvent(Vector2 position, Quaternion rotation) =>
        OnPlayerShot?.Invoke(position, rotation);

    private void Awake()
    {
        PlayerModelObject = transform.GetChild(0).gameObject;
        UsernameText = PlayerModelObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        SetUsername("");
    }
    
    private void SetUsername() =>
        UsernameText.text = Username; 

    private void SetUsername(string username) =>
        UsernameText.text = username;

    public void TookDamage(int damage, int currentHealth)
    {
        DamageNumManager.Instance.Create(PlayerModelObject.transform.position, damage, PhysicsHelper.RandomBool());
        OnPlayerTookDamage?.Invoke(damage, currentHealth);
    }

    internal void SetArmPositionRotation(Vector2 position, Quaternion rotation) =>
        OnArmPositionRotation?.Invoke(position, rotation);
}
