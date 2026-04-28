using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public WaveData[] waves;
    public int currentWaveIndex = 0;

    public Transform player;

    [Header("Map Bounds")]
    public Renderer backgroundRenderer;

    [Header("Spawn Settings")]
    public float spawnPadding = 1f;

    private Camera cam;
    private bool waveActive = false;

    public static event System.Action OnWaveCleared;

    void Start()
    {
        cam = Camera.main;
    }

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
            NextWave();
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
                Vector3 spawnPos = GetSpawnPosition();
                GameObject enemy = Instantiate(enemyData.enemyPrefab, spawnPos, Quaternion.identity);
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                    health.SetType(enemyData.enemyType);
            }
        }
    }

    void NextWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= 1 && currentWaveIndex <= 3)
            OnWaveCleared?.Invoke();

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
        Bounds map = backgroundRenderer.bounds;

        float halfH = cam.orthographicSize + spawnPadding;
        float halfW = cam.orthographicSize * cam.aspect + spawnPadding;
        Vector3 camPos = cam.transform.position;

        // Pick a point just outside one of the four camera edges
        Vector3 spawnPos;
        switch (Random.Range(0, 4))
        {
            case 0:  spawnPos = new Vector3(Random.Range(camPos.x - halfW, camPos.x + halfW), camPos.y + halfH, 0); break; // top
            case 1:  spawnPos = new Vector3(Random.Range(camPos.x - halfW, camPos.x + halfW), camPos.y - halfH, 0); break; // bottom
            case 2:  spawnPos = new Vector3(camPos.x - halfW, Random.Range(camPos.y - halfH, camPos.y + halfH), 0); break; // left
            default: spawnPos = new Vector3(camPos.x + halfW, Random.Range(camPos.y - halfH, camPos.y + halfH), 0); break; // right
        }

        // Clamp inside map bounds
        spawnPos.x = Mathf.Clamp(spawnPos.x, map.min.x, map.max.x);
        spawnPos.y = Mathf.Clamp(spawnPos.y, map.min.y, map.max.y);

        return spawnPos;
    }
}
