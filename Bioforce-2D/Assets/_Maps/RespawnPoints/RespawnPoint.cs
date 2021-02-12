using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Unity.Mathematics;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public static RespawnPoint Instance { get; set; }
    public Vector3 LastRespawnPoint;
    public Vector3 LastDiePosition;

    private Vector3[] Points;
    private System.Random Random = new System.Random();

    public static Vector3 GetRandomSpawnPoint(Vector3 diePosition)
    {
        int randomIndex = Instance.Random.Next(0, Instance.Points.Length);

        Instance.LastRespawnPoint = Instance.Points[randomIndex];
        Instance.LastDiePosition = diePosition;

        return Instance.LastRespawnPoint;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"RespawnPoint instance already exists, destroying {gameObject.name}");
            Destroy(gameObject);
        }

        int numPoints = transform.childCount;
        Points = new Vector3[numPoints];
        for (int count = 0; count < numPoints; count++)
            Points[count] = transform.GetChild(count).position;
    }


}
