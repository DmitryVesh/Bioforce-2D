using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonLocalPlayerHealth : MonoBehaviour, IHealth
{
    [SerializeField] private int MaxHealth = 50;
    [SerializeField] private int CurrentHealth = 0;

    //Set to default of -1, which will mean that the object will take damage from any bullet
    public int OwnerClientID { get; private set; } = -1;

    private PlayerManager PlayerManager { get; set; }

    public float GetMaxHealth() =>
        MaxHealth;
    public float GetCurrentHealth() =>
        CurrentHealth;

    public void TookDamage(int damage, int currentHealth)
    {
        //TODO: display hit effect
        CurrentHealth = currentHealth;
    }
    public void TakeDamage(int damage, int bulletOwnerID)
    {
        if (CurrentHealth - damage <= 0)
            Die(bulletOwnerID);

        PlayerManager.TookDamage(damage, CurrentHealth - damage);
        ClientSend.TookDamage(damage, CurrentHealth);
    }

    public void SetOwnerClientID(int iD) =>
        OwnerClientID = iD;

    public int GetOwnerClientID() => 
        OwnerClientID;

    public void Die(int bulletOwnerID)
    {
        GameManager.Instance.PlayerDied(OwnerClientID, bulletOwnerID, TypeOfDeath.Bullet);
        ScoreboardManager.Instance.AddKill(bulletOwnerID);
        ScoreboardManager.Instance.AddDeath(OwnerClientID);
        StartCoroutine(WaitBeforeRespawning());
    }
    public void Respawn()
    {
        ResetHealth();
        GameManager.Instance.PlayerRespawned(OwnerClientID);
    }
    public void FallDie()
    {
        GameManager.Instance.PlayerDied(OwnerClientID, OwnerClientID, TypeOfDeath.Fall);
        ScoreboardManager.Instance.AddDeath(OwnerClientID);
        StartCoroutine(WaitBeforeRespawning());
    }

    private void Start()
    {
        ResetHealth();
        PlayerManager = GameManager.PlayerDictionary[OwnerClientID];
        PlayerManager.OnPlayerTookDamage += TookDamage;
        PlayerManager.OnPlayerRespawn += ResetHealth;
    }
    private IEnumerator WaitBeforeRespawning()
    {
        yield return new WaitForSeconds(PlayerManager.DeadTime);
        Respawn();
    }
    protected void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }
}
