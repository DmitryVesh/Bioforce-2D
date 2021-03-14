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

    [SerializeField] private AudioClip[] PickupBandage;
    [SerializeField] private AudioClip[] PickupMedkit;

    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
        PlayerManager = GetComponentInParent<PlayerManager>();

        PlayerManager.OnPlayerShot += PlayShootSound;
        PlayerManager.OnPlayerJumped += PlayJumpSound;

        PlayerManager.OnPlayerTookDamage += PlayGotHitSound;
        PlayerManager.OnPlayerDeath += PlayDied;

        PlayerManager.OnPlayerPickupMedkit += PlayMedkitSound;
        PlayerManager.OnPlayerPickupBandage += PlayBandageSound;
    }
    private void OnDestroy()
    {
        PlayerManager.OnPlayerShot -= PlayShootSound;
        PlayerManager.OnPlayerJumped -= PlayJumpSound;

        PlayerManager.OnPlayerTookDamage -= PlayGotHitSound;
        PlayerManager.OnPlayerDeath -= PlayDied;

        PlayerManager.OnPlayerPickupMedkit -= PlayMedkitSound;
        PlayerManager.OnPlayerPickupBandage -= PlayBandageSound;
    }

    private void PlayMedkitSound(int integer) =>
        Play(PickupMedkit);
    private void PlayBandageSound(int integer) =>
        Play(PickupBandage);

    public void PlayShootSound(Vector2 position, Quaternion rotation) =>
        Play(Shoot);
    public void AnimatorFootstepSound() =>
        Play(Footsteps);
    public void PlayJumpSound() =>
        Play(Jump);
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
    private void Play(AudioClip[] audioClips) =>
        Play(SoundMusicManager.GetRandomAudioClip(audioClips));
    
}
