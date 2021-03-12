using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] public int ID;
    [SerializeField] private string Username;

    public Color PlayerColor { get; private set; }

    public Vector2 RespawnPosition { get; set; }
    public GameObject PlayerModelObject { get; private set; }
    [SerializeField] private TextMeshProUGUI UsernameText;

    // Delegate and event used to notify when movement stats are read from server
    public delegate void PlayerMovementStats(float runSpeed);
    public event PlayerMovementStats OnPlayerMovementStatsChanged;

    public delegate void PlayerSpeedX(float speedX);
    public event PlayerSpeedX OnPlayerSpeedXChanged;

    public delegate void NoParams();
    public event NoParams OnPlayerRespawn;
    public delegate void PlayerDeath(TypeOfDeath typeOfDeath);
    public event PlayerDeath OnPlayerDeath;
    public static float RespawnTime { get; private set; } = 1.5f;
    public static float DeadTime { get; private set; } = 3;

    public delegate void PlayerTookDamage(int damage, int currentHealth);
    public event PlayerTookDamage OnPlayerTookDamage;

    public delegate void Vector2andQuaternion(Vector2 position, Quaternion rotation);
    public event Vector2andQuaternion OnPlayerShot;
    
    public event NoParams OnPlayerJumped;

    public delegate void PlayerPosition(Vector2 position);
    public event PlayerPosition OnPlayerPosition;
    public delegate void PlayerRotation(Quaternion rotation);
    public event PlayerRotation OnPlayerRotation;

    public event Vector2andQuaternion OnArmPositionRotation;

    public delegate void Bool(bool boolean);
    public event Bool OnPlayerPaused;

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

    public void Initialise(int iD, string username, Color playerColor) 
    {
        (ID, Username) = (iD, username);
        SetUsername(playerColor);
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
        SetUsername("");
    }

    private void SetUsername(Color playerColor)
    {
        UsernameText.text = Username;
        PlayerColor = playerColor;
        UsernameText.color = PlayerColor;
    }

    private void SetUsername(string username) =>
        UsernameText.text = username;

    public void TookDamage(int damage, int currentHealth)
    {
        DamageNumManager.Instance.Create(PlayerModelObject.transform.position, damage, PhysicsHelper.RandomBool());
        OnPlayerTookDamage?.Invoke(damage, currentHealth);
    }

    internal void SetArmPositionRotation(Vector2 position, Quaternion rotation) =>
        OnArmPositionRotation?.Invoke(position, rotation);

    internal void SetPlayerPaused(bool paused) =>
        OnPlayerPaused?.Invoke(paused);
}
