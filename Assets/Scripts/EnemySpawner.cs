using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public int enemiesPerWave = 12;
    public float spawnDistance = 15f;
    public float groupSpacing = 1.2f;

    public Transform player;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        InvokeRepeating(nameof(SpawnWave), 2f, 5f);
    }

    void SpawnWave()
    {
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;

        Vector3 spawnCenter = player.position + (Vector3)(spawnDirection * spawnDistance);

        SpawnGroup(spawnCenter);
    }

    void SpawnGroup(Vector3 center)
    {
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(enemiesPerWave));

        int count = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (count >= enemiesPerWave)
                    return;

                Vector3 offset = new Vector3(
                    (x - gridSize / 2) * groupSpacing,
                    (y - gridSize / 2) * groupSpacing,
                    0
                );

                Instantiate(enemyPrefab, center + offset, Quaternion.identity);

                count++;
            }
        }
    }
}