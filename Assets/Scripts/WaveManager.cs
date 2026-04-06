using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public WaveData[] waves;
    public int currentWaveIndex = 0;

    public Transform player;
    public BoxCollider2D mapBounds;

    //public float spawnDistance = Random.Range(8, 15);
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
                //wait
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
        //float x = Random.Range(-14, 14);
        //float y = Random.Range(-11, 11);
        float spawnDistance = Random.Range(8, 15);
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;

        Vector3 spawnCenter = player.position + (Vector3)(spawnDirection * spawnDistance);

        //FUCKED UP HARDCODING BUT THIS IS THE WALL BOUNDARIES
        if (spawnCenter.x < -15)
        {
            spawnCenter.x = -11;
        }
        else if (spawnCenter.x > 15)
        {
            spawnCenter.x = 11;
        }
        if (spawnCenter.y < -15)
        {
            spawnCenter.y = -11;
        }
        else if (spawnCenter.y > 15)
        {
            spawnCenter.y = 11;
        }
        //SpawnGroup(spawnCenter);

        //return new Vector3(x, y, 0);
        return spawnCenter;
    }
}