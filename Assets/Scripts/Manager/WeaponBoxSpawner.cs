using UnityEngine;

public class WeaponBoxSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject weaponBoxPrefab;

    [Header("Spawn Settings")]
    public float spawnOffsetX = 2f;

    void Start()
    {
        SpawnBox();
    }

    void OnEnable()
    {
        WaveManager.OnWaveCleared += SpawnBox;
    }

    void OnDisable()
    {
        WaveManager.OnWaveCleared -= SpawnBox;
    }

    void SpawnBox()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Vector3 spawnPos = player.transform.position + new Vector3(spawnOffsetX, 0f, 0f);
        Instantiate(weaponBoxPrefab, spawnPos, Quaternion.identity);
    }
}
