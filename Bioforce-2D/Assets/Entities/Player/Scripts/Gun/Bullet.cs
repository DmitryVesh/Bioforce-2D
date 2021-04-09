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
    private byte OwnerClientID = 255;
    private bool Available = false;

    private Animator Animator { get; set; }
    public CapsuleCollider2D Hitbox { get; private set; }
    private SpriteRenderer Sprite { get; set; }
    private Color BulletColor { get; set; }
    private ParticleSystem ParticleSystem { get; set; }

    [SerializeField] private float TimeBeforeShrinking = 0.3f;
    [SerializeField] private float TimeToShrink = 0.7f;

    [SerializeField] private Vector3 ShrinkTo = new Vector3(0.3f, 0.3f, 0.3f);
    private Vector3 OriginalScale { get; set; }


    public bool IsAvailable()
    {
        return Available;
    }
    public void SetBulletColor(Color playerColor)
    {
        BulletColor = playerColor;
    }

    public void SetOwner(byte iD) =>
        OwnerClientID = iD;
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
        ParticleSystem = GetComponent<ParticleSystem>();
        gameObject.SetActive(false);

        OriginalScale = transform.localScale;
    }
    private void FixedUpdate()
    {
        if (!Available)
        {
            DecreaseSizeOverTime();

            CurrentTimeToLive -= Time.fixedDeltaTime;
            if (CurrentTimeToLive < 0)
                ResetBullet();
        }
    }

    private void DecreaseSizeOverTime()
    {
        float CurrentLiveTime = TimeToLive - CurrentTimeToLive;
        if (CurrentLiveTime > TimeBeforeShrinking)
            return;

        transform.localScale = Vector3.Lerp(transform.localScale, ShrinkTo, CurrentLiveTime / TimeToShrink);
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
                float damageRatio = transform.localScale.magnitude / OriginalScale.magnitude;
                int damage = (int)((float)Random.Range(DamageMin, DamageMax + 1) * damageRatio);
                health.TakeDamage(damage, OwnerClientID);
            }
            else if (ownBullet)
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
        ParticleSystem.Play();

        GameManager.PlayerDictionary[OwnerClientID].PlayersBulletHitCollider();
    }
    private void ResetBullet()
    {
        gameObject.SetActive(false);
        Available = true;
        transform.localScale = OriginalScale;
    }
}
