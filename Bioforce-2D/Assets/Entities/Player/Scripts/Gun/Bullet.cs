using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float Speed = 30;
    [SerializeField] private int DamageMin = 5;
    [SerializeField] private int DamageMax = 15;
    [SerializeField] private float TimeToLive = 1;
    private float CurrentTimeToLive;
    //TODO: make a pooling system for impactEffect as well
    //[SerializeField] private GameObject impactEffect;
    private Rigidbody2D rb;
    private int OwnerClientID = -1;
    private bool Available = false;

    public bool IsAvailable()
    {
        return Available;
    }
    public void SetOwnerClientID(int iD)
    {
        OwnerClientID = iD;
    }
    public void Shoot(Vector2 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);

        rb.velocity = transform.right * Speed;
        CurrentTimeToLive = TimeToLive;
        Available = false;
    }

    private void Awake()
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"));
        rb = GetComponent<Rigidbody2D>();
        gameObject.SetActive(false);
    }
    private void FixedUpdate()
    {
        if (!Available)
        {
            CurrentTimeToLive -= Time.fixedDeltaTime;
            if (CurrentTimeToLive < 0)
                ResetBullet();
        }
    }


    // Preventing mutliple hits and etc
    private void OnCollisionEnter2D(Collision2D collision)
    {
        IHealth health = collision.transform.GetComponentInParent<IHealth>();
        if (health != null)
        {
            int healthOwner = health.GetOwnerClientID();
            //So only hurts local player, so other people's bullets only hurt you,
            if (healthOwner == Client.Instance.ClientID && healthOwner != OwnerClientID)
            {
                int damage = Random.Range(DamageMin, DamageMax + 1);
                health.TakeDamage(damage, OwnerClientID);
            }
        }
        BulletHit();
    }
    private void BulletHit()
    {
        //TODO: 2 Instantiate(impactEffect, transform.position, transform.rotation);
        ResetBullet();
    }
    private void ResetBullet()
    {
        gameObject.SetActive(false);
        CurrentTimeToLive = TimeToLive;
        Available = true;
    }
}
