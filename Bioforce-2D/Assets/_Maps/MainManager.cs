using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    private void Awake()
    {
        int childCount = transform.childCount;
        GameObject[] children = new GameObject[childCount];
        for (int count = 0; count < childCount; count++)
        {
            children[count] = transform.GetChild(count).gameObject;
        }
        transform.DetachChildren();

        foreach (GameObject child in children)
        {
            child.SetActive(true);
            DontDestroyOnLoad(child);
        }
    }
}
