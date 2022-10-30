using UnityEngine;
using UnityEngine.Singleton;

public class RespawnPoint : MonoBehaviour
{
    public static RespawnPoint Instance { get => instance; }
    private static RespawnPoint instance;

    private Vector3[] Points;

    public static Vector2 GetRandomSpawnPoint()
    {
        int randomIndex = Random.Range(0, Instance.Points.Length);
        return Instance.Points[randomIndex];
    }

    private void Awake()
    {
        Singleton.Init(ref instance, this);        
    }
    private void Start()
    {
        int numPoints = transform.childCount;
        Points = new Vector3[numPoints];
        for (int count = 0; count < numPoints; count++)
            Points[count] = transform.GetChild(count).position;
    }


}