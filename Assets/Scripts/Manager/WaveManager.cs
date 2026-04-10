using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public WaveData[] waves;
    public int currentWaveIndex = 0;

    public Transform player;
    public BoxCollider2D mapBounds;

    private bool waveActive = false;

    public static event System.Action OnWaveCleared;

    void OnEnable()
    {
        WeaponController.OnFirstWeaponEquipped += StartFirstWave;
    }

    void OnDisable()
    {
        WeaponController.OnFirstWeaponEquipped -= StartFirstWave;
    }

    void Update()
    {
        if (!waveActive) return;

        if (EnemyManager.Instance.AreAllEnemiesDead())
        {
            NextWave();
        }
    }

    void StartFirstWave()
    {
        waveActive = true;
        StartWave();
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
            }
        }
    }

    void NextWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= 1 && currentWaveIndex <= 3)
        {
            OnWaveCleared?.Invoke();
        }
        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log("All waves cleared!");
            waveActive = false;
            return;
        }

        StartWave();
    }

    Vector3 GetSpawnPosition()
    {
        float spawnDistance = Random.Range(8, 15);
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;

        Vector3 spawnCenter = player.position + (Vector3)(spawnDirection * spawnDistance);

        //HARDCODED FOR NOW FIX!!
        if (spawnCenter.x < -15) spawnCenter.x = -11;
        else if (spawnCenter.x > 15) spawnCenter.x = 11;
        if (spawnCenter.y < -15) spawnCenter.y = -11;
        else if (spawnCenter.y > 15) spawnCenter.y = 11;

        return spawnCenter;
    }
}
