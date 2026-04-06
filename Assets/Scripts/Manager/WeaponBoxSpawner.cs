using UnityEngine;

public class WeaponBoxSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject weaponBoxPrefab;

    [Header("Spawn Settings")]
    public Transform spawnPoint;

    void Start()
    {
        SpawnBox();
    }

    void OnEnable()
    {
        WaveManager.OnFirstWaveCleared += SpawnBox;
    }

    void OnDisable()
    {
        WaveManager.OnFirstWaveCleared -= SpawnBox;
    }

    void SpawnBox()
    {
        Instantiate(weaponBoxPrefab, spawnPoint.position, Quaternion.identity);
    }
}
