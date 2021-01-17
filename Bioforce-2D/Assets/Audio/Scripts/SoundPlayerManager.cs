using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayerManager : MonoBehaviour
{
    private AudioSource AudioSource { get; set; }
    private PlayerManager PlayerManager { get; set; }

    [SerializeField] private AudioClip[] Shoot = null;
    [SerializeField] private AudioClip[] Jump;
    [SerializeField] private AudioClip[] Footsteps;

    [SerializeField] private AudioClip WasHit;
    [SerializeField] private AudioClip DieBullet;
    [SerializeField] private AudioClip DieFall;

    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
        PlayerManager = GetComponentInParent<PlayerManager>();
        PlayerManager.OnPlayerShot += PlayShootSound;
        PlayerManager.OnPlayerJumped += PlayJumpSound;
        PlayerManager.OnPlayerTookDamage += PlayGotHitSound;
        PlayerManager.OnPlayerDeath += PlayDied;
    }

    public void PlayShootSound(Vector2 position, Quaternion rotation) =>
        Play(SoundMusicManager.GetRandomAudioClip(Shoot));
    public void AnimatorFootstepSound() =>
        Play(SoundMusicManager.GetRandomAudioClip(Footsteps));
    public void PlayJumpSound() =>
        Play(SoundMusicManager.GetRandomAudioClip(Jump));
    public void PlayGotHitSound(int damage, int bulletOwnerID) =>
        Play(WasHit);
    public void PlayDied(TypeOfDeath typeOfDeath)
    {
        if (typeOfDeath == TypeOfDeath.Bullet)
            Play(DieBullet);
        else if (typeOfDeath == TypeOfDeath.Fall)
            Play(DieFall);
    }

    private void Play(AudioClip audioClip) =>
        AudioSource.PlayOneShot(audioClip);
    
}
