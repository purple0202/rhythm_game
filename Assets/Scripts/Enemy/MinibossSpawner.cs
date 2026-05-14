using System;
using UnityEditor.MPE;
using UnityEngine;

public class MinibossSpawner : MonoBehaviour
{
    [SerializeField] GameObject minibossPrefab;
    [SerializeField] KeyCode spawnKey = KeyCode.M;

    void Update()
    {
        if (!Input.GetKeyDown(spawnKey))
        {
            return;
        }

        if (minibossPrefab == null)
        {
            Debug.LogWarning("MinibossSpawner: minibossPrefab is not assigned in the Inspector.");
            return;
        }

        Instantiate(minibossPrefab, Vector3.zero, Quaternion.identity);
    }
}
