using System;
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
    //[SerializeField] private GameObject impactEffect;
    private Rigidbody2D rb;
    private int OwnerClientID = -1;
    private bool Available = false;

    private Animator Animator { get; set; }
    private CapsuleCollider2D Hitbox { get; set; }
    private SpriteRenderer Sprite { get; set; }
    private Color BulletColor { get; set; }

    public bool IsAvailable()
    {
        return Available;
    }
    public void SetBulletColor(Color playerColor)
    {
        BulletColor = playerColor;
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
        Animator.SetTrigger("shot");
        Sprite.color = BulletColor;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = transform.right * Speed;
        CurrentTimeToLive = TimeToLive;
        Available = false;
        Hitbox.enabled = true;
    }

    private void Awake()
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"));
        rb = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Hitbox = GetComponent<CapsuleCollider2D>();
        Sprite = GetComponent<SpriteRenderer>();
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


    private void OnCollisionEnter2D(Collision2D collision)
    {
        IHealth health = collision.transform.GetComponentInParent<IHealth>();
        if (health != null)
        {
            int healthOwner = health.GetOwnerClientID();

            bool ownClient = healthOwner == Client.Instance.ClientID;
            bool ownBullet = healthOwner == OwnerClientID;
            //So only hurts local player, so other people's bullets only hurt you,
            if (ownClient && !ownBullet)
            {
                int damage = UnityEngine.Random.Range(DamageMin, DamageMax + 1);
                health.TakeDamage(damage, OwnerClientID);
            }
            else if (ownClient && ownBullet)
                return;
        }
        BulletHit();
    }
    private void BulletHit()
    {
        float impactEffectTime = 0.4f;
        Invoke("ResetBullet", impactEffectTime);
        Sprite.color = Color.white;
        Animator.SetTrigger("hit");
        Hitbox.enabled = false;
        CurrentTimeToLive = impactEffectTime;
        rb.bodyType = RigidbodyType2D.Static;
    }
    private void ResetBullet()
    {
        gameObject.SetActive(false);
        Available = true;
    }
}
