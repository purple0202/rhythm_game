using UnityEngine;

public class WeaponBoxSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject weaponBoxPrefab;

    [Header("Spawn Settings")]
    public float spawnOffsetX = 2f;

    [Header("Box Data")]
    public WeaponDropData dropData;
    public WeaponGroupData[] groupPool;

    int boxesSpawned;

    void Start() => SpawnBox();

    void OnEnable() => WaveManager.OnWaveCleared += SpawnBox;
    void OnDisable() => WaveManager.OnWaveCleared -= SpawnBox;

    void SpawnBox()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        WeaponBoxData boxData;
        if (boxesSpawned == 0)
        {
            boxData = dropData;
        }
        else
        {
            if (groupPool == null || groupPool.Length == 0) return;
            boxData = groupPool[Random.Range(0, groupPool.Length)];
        }

        if (boxData == null) return;
        boxesSpawned++;

        Vector3 spawnPos = player.transform.position + new Vector3(spawnOffsetX, 0f, 0f);
        GameObject go = Instantiate(weaponBoxPrefab, spawnPos, Quaternion.identity);
        go.GetComponent<WeaponBox>().Initialize(boxData);
    }
}
