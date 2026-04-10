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
        //Change spawn coordinates so that it doesn't automatically hit the player
        Instantiate(weaponBoxPrefab, spawnPoint.position, Quaternion.identity);
    }
}
