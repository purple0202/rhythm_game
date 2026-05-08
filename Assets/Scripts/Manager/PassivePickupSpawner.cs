using UnityEngine;

public class PassivePickupSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject passivePickupPrefab;

    [Header("Spawn Settings")]
    public float spawnOffsetX = -2f;
    public int spawnAfterWave = 1;

    int wavesClearedCount = 0;

    void OnEnable()  => WaveManager.OnWaveCleared += OnWaveCleared;
    void OnDisable() => WaveManager.OnWaveCleared -= OnWaveCleared;

    void OnWaveCleared()
    {
        wavesClearedCount++;
        if (wavesClearedCount != spawnAfterWave) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Vector3 spawnPos = player.transform.position + new Vector3(spawnOffsetX, 0f, 0f);
        Instantiate(passivePickupPrefab, spawnPos, Quaternion.identity);
    }
}
