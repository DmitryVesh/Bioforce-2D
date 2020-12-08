using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonLocalPlayerHealth : MonoBehaviour, IHealth
{
    [SerializeField] private int MaxHealth = 50;
    [SerializeField] private int CurrentHealth = 0;

    //Set to default of -1, which will mean that the object will take damage from any bullet
    public int OwnerClientID { get; private set; } = -1;

    public void TakeDamage(int damage, int bulletOwnerID)
    {
        CurrentHealth -= damage;
        //TODO: display hit effect
        if (CurrentHealth <= 0) 
            Die(bulletOwnerID);
    }
    public void SetOwnerClientID(int iD)
    {
        OwnerClientID = iD;
    }
    public int GetOwnerClientID()
    {
        return OwnerClientID;
    }

    public void Die(int bulletOwnerID)
    {
        GameManager.Instance.PlayerDied(OwnerClientID, bulletOwnerID);
        StartCoroutine(WaitBeforeRespawning());
    }
    public void Respawn()
    {
        ResetHealth();
        GameManager.Instance.PlayerRespawned(OwnerClientID);
    }
    public void FallDie()
    {
        GameManager.Instance.PlayerDied(OwnerClientID, OwnerClientID);
        StartCoroutine(WaitBeforeRespawning());
    }

    private void Start()
    {
        ResetHealth();
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
