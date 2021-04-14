using System;
using UnityEngine;
using UnityEngine.Audio;

public class SoundPlayerManager : MonoBehaviour
{
    [SerializeField] private AudioSource AudioSource;
    private PlayerManager PlayerManager { get; set; }

    [SerializeField] private AudioClip[] Shoot;
    [SerializeField] private AudioClip[] BulletHit;
    [SerializeField] private AudioClip[] Jump;
    [SerializeField] private AudioClip[] Footsteps;

    [SerializeField] private AudioClip WasHit;
    [SerializeField] private AudioClip DieBullet;
    [SerializeField] private AudioClip DieFall;

    [SerializeField] private AudioClip[] PickupBandage;
    [SerializeField] private AudioClip[] PickupMedkit;
    [SerializeField] private AudioClip[] PickupAdrenaline;

    [SerializeField] private AudioClip HitMarker;

    [SerializeField] private AudioClip[] SkullLaughing;

    [SerializeField] private AudioSource HeartBeatAudioSource; //Must have its own to adjust tempo, pitch and volume
    [SerializeField] private float MinHeartBeatVolume = 0.5f;
    [SerializeField] private float MaxHeartBeatVolume = 1f;
    [SerializeField] private AudioMixerGroup HeartbeatMixer;
    [SerializeField] private float MinHeartBeatTempo = 1f;
    [SerializeField] private float MaxHeartBeatTempo = 1.75f;
    private bool PlayingHeartbeat;

    const float NormalPitch = 1f;

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

        PlayerManager.OnPlayersBulletHitCollider += BulletHitCollider;

        PlayerManager.OnLocalPlayerHitAnother += HitMarkerSound;

        PlayerManager.OnPlayerDeath += PlaySkullLaughing;

        PlayerManager.OnHeartBeatShouldPlay += PlayHeartBeat;
        PlayerManager.OnPlayerDeath += StopHeartBeat;

        PlayerManager.OnPlayerPickupAdrenaline += PlayAdrenalineSound;
    }

    private void PlayHeartBeat(int health, int healthMinToPlayHeartBeat, int healthMaxToPlayHeartBeat)
    {
        if (health > healthMinToPlayHeartBeat)
        {
            StopHeartBeat(TypeOfDeath.Bullet);
            return;
        }

        HeartBeatAudioSource.volume = LinearInterpolate(health, healthMinToPlayHeartBeat, healthMaxToPlayHeartBeat, MinHeartBeatVolume, MaxHeartBeatVolume);

        //TODO: Turn down the music volume as well to amplify effect
        float tempo = LinearInterpolate(health, healthMinToPlayHeartBeat, healthMaxToPlayHeartBeat, MinHeartBeatTempo, MaxHeartBeatTempo);
        HeartBeatAudioSource.pitch = tempo;
        HeartbeatMixer.audioMixer.SetFloat("Heartbeat Pitch", 1f / tempo);

        if (!PlayingHeartbeat)
        {
            HeartBeatAudioSource.Play();
            PlayingHeartbeat = true;
        }
    }
    private void StopHeartBeat(TypeOfDeath _)
    {
        if (PlayingHeartbeat)
        {
            HeartBeatAudioSource.Stop();
            PlayingHeartbeat = false;
        }
    }

    private static float LinearInterpolate(int health, int healthMinToPlayHeartBeat, int healthMaxToPlayHeartBeat, float minVal, float maxVal)
    {
        int diffRanges = healthMaxToPlayHeartBeat - healthMinToPlayHeartBeat;
        int diffCurrent = health - healthMinToPlayHeartBeat;

        float diffVals = maxVal - minVal;

        return ((float)diffCurrent / (float)diffRanges) * diffVals + minVal;
    }

    private void PlaySkullLaughing(TypeOfDeath typeOfDeath)
    {
        if (typeOfDeath.Equals(TypeOfDeath.Bullet))
            Play(SkullLaughing);
    }

    private void OnDestroy()
    {
        PlayerManager.OnPlayerShot -= PlayShootSound;
        PlayerManager.OnPlayerJumped -= PlayJumpSound;

        PlayerManager.OnPlayerTookDamage -= PlayGotHitSound;
        PlayerManager.OnPlayerDeath -= PlayDied;

        PlayerManager.OnPlayerPickupMedkit -= PlayMedkitSound;
        PlayerManager.OnPlayerPickupBandage -= PlayBandageSound;

        PlayerManager.OnPlayersBulletHitCollider -= BulletHitCollider;

        PlayerManager.OnLocalPlayerHitAnother -= HitMarkerSound;

        PlayerManager.OnPlayerDeath -= PlaySkullLaughing;

        PlayerManager.OnHeartBeatShouldPlay -= PlayHeartBeat;
        PlayerManager.OnPlayerDeath -= StopHeartBeat;

        PlayerManager.OnPlayerPickupAdrenaline -= PlayAdrenalineSound;
    }

    

    //Called by Bullet Class
    public void BulletHitCollider() =>
        Play(BulletHit);

    private void HitMarkerSound() =>
        Play(HitMarker);
    private void PlayMedkitSound(int _) =>
        Play(PickupMedkit);
    private void PlayBandageSound(int _) =>
        Play(PickupBandage);
    private void PlayAdrenalineSound(float floatVal) =>
        Play(PickupAdrenaline);

    public void PlayShootSound(Vector2 position, Quaternion rotation) =>
        Play(Shoot);
    public void AnimatorFootstepSound() =>
        Play(Footsteps);
    public void PlayJumpSound() =>
        Play(Jump);
    public void PlayGotHitSound(int currentHealth) =>
        Play(WasHit);
    public void PlayDied(TypeOfDeath typeOfDeath)
    {
        if (typeOfDeath == TypeOfDeath.Bullet)
            Play(DieBullet);
        else if (typeOfDeath == TypeOfDeath.Fall)
            Play(DieFall);
    }

    private void Play(AudioClip audioClip)
    {
        AudioSource.pitch = NormalPitch;
        AudioSource.PlayOneShot(audioClip);
    }
    private void Play(AudioClip[] audioClips)
    {
        Play(SoundMusicManager.GetRandomAudioClip(audioClips));
        AudioSource.pitch = UnityEngine.Random.Range(0.85f, 1.15f);
    }
    
}
