using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonLocalPlayerHealth : MonoBehaviour, IHealth
{
    [SerializeField] private int MaxHealth = 0;
    [SerializeField] protected int CurrentHealth = 0;

    //Set to default of -1, which will mean that the object will take damage from any bullet
    public byte OwnerClientID { get; private set; } = 255;

    protected PlayerManager PlayerManager { get; set; }
    protected bool CantGetHit { get; set; }

    public bool CanTakeDamage() =>
        !CantGetHit;

    public float GetMaxHealth() =>
        MaxHealth;
    public float GetCurrentHealth() =>
        CurrentHealth;
    public void SetSpawnedHealth(int maxHealth, int currentHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth;
    }
    public void ResetHealth() =>
        CurrentHealth = MaxHealth;

    public void TookDamage(int damage, int currentHealth)
    {
        CurrentHealth = currentHealth;
    }
    public virtual void TakeDamage(int damage, byte bulletOwnerID) { }

    public void SetOwnerClientID(byte iD) =>
        OwnerClientID = iD;

    public byte GetOwnerClientID() => 
        OwnerClientID;

    public void Die(byte bulletOwnerID)
    {
        CantGetHit = true;
        GameManager.Instance.PlayerDied(OwnerClientID, bulletOwnerID, TypeOfDeath.Bullet);
        ScoreboardManager.Instance.AddKill(bulletOwnerID);
        ScoreboardManager.Instance.AddDeath(OwnerClientID);
        StartCoroutine(WaitBeforeRespawning());
    }
    public void Respawn()
    {
        ResetHealth();
        Invoke("SetCantGetHitFalse", PlayerManager.RespawnTime);
        GameManager.Instance.PlayerRespawned(OwnerClientID);
    }
    private void SetCantGetHitFalse() =>
        CantGetHit = false;

    public void FallDie()
    {
        GameManager.Instance.PlayerDied(OwnerClientID, OwnerClientID, TypeOfDeath.Fall);
        ScoreboardManager.Instance.AddDeath(OwnerClientID);
        StartCoroutine(WaitBeforeRespawning());
    }

    private void Start()
    {
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];
        PlayerManager.OnPlayerTookDamage += TookDamage;
        PlayerManager.OnPlayerRespawn += ResetHealth;

        PlayerManager.OnPlayerPickupMedkit += PickupHealth;
        PlayerManager.OnPlayerPickupBandage += PickupHealth;
    }
    private void OnDestroy()
    {
        PlayerManager.OnPlayerTookDamage -= TookDamage;
        PlayerManager.OnPlayerRespawn -= ResetHealth;

        PlayerManager.OnPlayerPickupMedkit -= PickupHealth;
        PlayerManager.OnPlayerPickupBandage -= PickupHealth;
    }

    private void PickupHealth(int restoreHealth)
    {
        CurrentHealth += restoreHealth;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
    }

    private IEnumerator WaitBeforeRespawning()
    {
        yield return new WaitForSeconds(PlayerManager.DeadTime);
        Respawn();
    }

}
