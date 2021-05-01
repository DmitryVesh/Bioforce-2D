using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;

public class RespawnPoint : MonoBehaviour
{
    public static RespawnPoint Instance { get => instance; }
    private static RespawnPoint instance;

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
        Singleton.Init(ref instance, this);

        int numPoints = transform.childCount;
        Points = new Vector3[numPoints];
        for (int count = 0; count < numPoints; count++)
            Points[count] = transform.GetChild(count).position;
    }


}
