using UnityEngine;
using System.Collections.Generic;

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
            WeaponController controller = player.GetComponentInChildren<WeaponController>();
            if (controller == null) return;

            // Same exclusivity rule as pickup: skip colours already obtained. If that empties
            // the pool, the player has all weapons, so no box spawns at all.
            List<WeaponGroupData> available = new List<WeaponGroupData>();
            if (groupPool != null)
                foreach (var group in groupPool)
                    if (group != null && !controller.IsGroupUsed(group.groupId))
                        available.Add(group);

            if (available.Count == 0) return;
            boxData = available[Random.Range(0, available.Count)];
        }

        if (boxData == null) return;
        boxesSpawned++;

        Vector3 spawnPos = player.transform.position + new Vector3(spawnOffsetX, 0f, 0f);
        GameObject go = Instantiate(weaponBoxPrefab, spawnPos, Quaternion.identity);
        go.GetComponent<WeaponBox>().Initialize(boxData);
    }
}
