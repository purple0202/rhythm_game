using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public WaveData[] waves;
    public int currentWaveIndex = 0;

    public Transform player;
    public BoxCollider2D mapBounds;

    void Start()
    {
        StartWave();
    }

    void Update()
    {
        if (EnemyManager.Instance.AreAllEnemiesDead())
        {
            NextWave();
        }
    }

    void StartWave()
    {
        WaveData wave = waves[currentWaveIndex];

        foreach (var enemyData in wave.enemies)
        {
            for (int i = 0; i < enemyData.count; i++)
            {
                Debug.Log("found enemy");
                Vector3 spawnPos = GetSpawnPosition();
                Instantiate(enemyData.enemyPrefab, spawnPos, Quaternion.identity);
                //EnemySpawner.SpawnWave();
            }
        }
    }

    void NextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log("All waves cleared!");
            return;
        }

        StartWave();
    }

    Vector3 GetSpawnPosition()
    {
        //Bounds bounds = mapBounds.bounds;

        //HARDCODED FOR NOW FIX!!
        float x = Random.Range(-14, 14);
        float y = Random.Range(-11, 11);

        return new Vector3(x, y, 0);
    }
}