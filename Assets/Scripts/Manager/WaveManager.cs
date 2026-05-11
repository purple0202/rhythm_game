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
    public float mapInset = 1.5f;

    [Tooltip("Wave size at which enemies use the full map spread.")]
    public int maxWaveSize = 20;
    [Tooltip("How far beyond the camera edge enemies can spawn for the smallest wave.")]
    public float minStripDepth = 2f;

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
        float mult = PlayerStats.Instance != null ? PlayerStats.Instance.enemySpawnMultiplier : 1f;

        int totalEnemies = 0;
        foreach (var enemyData in wave.enemies)
            totalEnemies += Mathf.RoundToInt(enemyData.count * mult);

        foreach (var enemyData in wave.enemies)
        {
            int count = Mathf.RoundToInt(enemyData.count * mult);
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetSpawnPosition(totalEnemies);
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

    Vector3 GetSpawnPosition(int waveSize)
    {
        Bounds map = backgroundRenderer.bounds;

        float insetMinX = map.min.x + mapInset;
        float insetMaxX = map.max.x - mapInset;
        float insetMinY = map.min.y + mapInset;
        float insetMaxY = map.max.y - mapInset;

        Vector3 camPos = cam.transform.position;
        float camHalfH = cam.orthographicSize + spawnPadding;
        float camHalfW = cam.orthographicSize * cam.aspect + spawnPadding;

        float camTop    = camPos.y + camHalfH;
        float camBottom = camPos.y - camHalfH;
        float camRight  = camPos.x + camHalfW;
        float camLeft   = camPos.x - camHalfW;

        // 0 = smallest wave (tight), 1 = maxWaveSize or larger (full spread)
        float t = Mathf.Clamp01((float)waveSize / maxWaveSize);

        // Depth: how far beyond the camera edge each strip extends
        float topMax    = Mathf.Lerp(camTop    + minStripDepth, insetMaxY, t);
        float bottomMin = Mathf.Lerp(camBottom - minStripDepth, insetMinY, t);
        float rightMax  = Mathf.Lerp(camRight  + minStripDepth, insetMaxX, t);
        float leftMin   = Mathf.Lerp(camLeft   - minStripDepth, insetMinX, t);

        // Width: how far along the edge enemies can spread (centered on camera)
        float halfW = Mathf.Lerp(camHalfW, (insetMaxX - insetMinX) * 0.5f, t);
        float halfH = Mathf.Lerp(camHalfH, (insetMaxY - insetMinY) * 0.5f, t);
        float xMin = Mathf.Clamp(camPos.x - halfW, insetMinX, insetMaxX);
        float xMax = Mathf.Clamp(camPos.x + halfW, insetMinX, insetMaxX);
        float yMin = Mathf.Clamp(camPos.y - halfH, insetMinY, insetMaxY);
        float yMax = Mathf.Clamp(camPos.y + halfH, insetMinY, insetMaxY);

        float x, y;
        switch (Random.Range(0, 4))
        {
            case 0: // top
                x = Random.Range(xMin, xMax);
                y = Random.Range(camTop, topMax);
                break;
            case 1: // bottom
                x = Random.Range(xMin, xMax);
                y = Random.Range(bottomMin, camBottom);
                break;
            case 2: // left
                x = Random.Range(leftMin, camLeft);
                y = Random.Range(yMin, yMax);
                break;
            default: // right
                x = Random.Range(camRight, rightMax);
                y = Random.Range(yMin, yMax);
                break;
        }

        return new Vector3(x, y, 0);
    }
}
