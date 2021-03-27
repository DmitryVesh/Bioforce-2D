using UnityEngine;

[System.Serializable]
public class LinkedParticleSystemManager
{
    [SerializeField] private GameObject ParticleGameObject;
    private ParticleSystem[] ParticleSystems;
    private Transform[] ParticleTransforms;
    [SerializeField] private int NumMaxParticleSystems = 10;
    private int CurrentParticleIndex { get; set; } = 0;

    
    public void PlayAffect(Vector2 position, Quaternion rotation)
    {
        Transform transform = ParticleTransforms[CurrentParticleIndex];
        transform.position = position;
        transform.rotation = rotation;

        ParticleSystem particle = ParticleSystems[CurrentParticleIndex];
        particle.Play();

        CurrentParticleIndex = (CurrentParticleIndex + 1) % NumMaxParticleSystems;
    }
    internal void Initilise(Color color, int ownerID)
    {
        ParticleSystems = new ParticleSystem[NumMaxParticleSystems];
        ParticleTransforms = new Transform[NumMaxParticleSystems];

        ParticleSystem mainParticleSystem = ParticleGameObject.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = mainParticleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color);
        GenerateParticleSystems(ownerID);
    }

    private void GenerateParticleSystems(int ownerID)
    {
        Transform particleHolder = new GameObject($"ParticleHolder: {ownerID}").transform;
        particleHolder.position = Vector3.zero;

        for (int particleCount = 0; particleCount < NumMaxParticleSystems; particleCount++)
        {
            GameObject particleSystem = GameObject.Instantiate(ParticleGameObject, particleHolder);
            ParticleTransforms[particleCount] = particleSystem.transform;
            ParticleSystems[particleCount] = particleSystem.GetComponent<ParticleSystem>();
        }
    }
}
