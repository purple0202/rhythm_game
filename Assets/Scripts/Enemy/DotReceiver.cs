using UnityEngine;

// Add this component to every enemy prefab that should receive DOT damage.
public class DotReceiver : MonoBehaviour
{
    [System.Serializable]
    public class DotConfig
    {
        public float damagePerTick = 2f;
        public int   maxTicks      = 5;
        public Color tintColor     = Color.white;
        public Color popupColor    = Color.white;
        // Space for future type-specific fields (e.g. chain chance, burn spread)
    }

    [Header("Fire")]
    public DotConfig fireConfig = new DotConfig
    {
        damagePerTick = 4f,
        maxTicks      = 5,
        tintColor     = new Color(1f, 0.35f, 0.05f),
        popupColor    = new Color(1f, 0.2f, 0.05f)
    };

    [Header("Electric")]
    public DotConfig electricConfig = new DotConfig
    {
        damagePerTick = 2f,
        maxTicks      = 5,
        tintColor     = new Color(0.4f, 0.6f, 1f),
        popupColor    = new Color(0.3f, 0.5f, 1f)
    };
    [Range(0f, 1f)] public float electricSlowPercent = 0.3f;

    // Read by movement scripts to apply slow.
    [HideInInspector] public float slowMultiplier = 1f;

    int fireTicks;
    int electricTicks;

    SpriteRenderer spriteRenderer;
    Color          originalColor;
    EnemyHealth    enemyHealth;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth    = GetComponent<EnemyHealth>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void OnEnable()  => BeatConductor.OnBeat += OnBeat;
    void OnDisable()
    {
        BeatConductor.OnBeat -= OnBeat;
        ResetVisuals();
    }

    public void Apply(DotType type)
    {
        switch (type)
        {
            case DotType.Fire:     fireTicks     = fireConfig.maxTicks;     break;
            case DotType.Electric: electricTicks = electricConfig.maxTicks; break;
        }
        UpdateVisuals();
    }

    void OnBeat()
    {
        if (Time.timeScale == 0f) return;

        if (fireTicks > 0)
        {
            DealDamage(fireConfig);
            fireTicks--;
        }

        if (electricTicks > 0)
        {
            DealDamage(electricConfig);
            electricTicks--;
        }

        UpdateVisuals();
    }

    void DealDamage(DotConfig config)
    {
        if (enemyHealth == null) return;
        Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(0.3f, 0.7f);
        enemyHealth.TakeDamage(config.damagePerTick, EnemyType.None, config.popupColor, offset);
    }

    void UpdateVisuals()
    {
        bool fire  = fireTicks     > 0;
        bool elec  = electricTicks > 0;

        slowMultiplier = elec ? 1f - electricSlowPercent : 1f;

        if (spriteRenderer == null) return;

        if (fire && elec)
            spriteRenderer.color = Color.Lerp(fireConfig.tintColor, electricConfig.tintColor, 0.5f);
        else if (fire)
            spriteRenderer.color = fireConfig.tintColor;
        else if (elec)
            spriteRenderer.color = electricConfig.tintColor;
        else
            ResetVisuals();
    }

    void ResetVisuals()
    {
        slowMultiplier = 1f;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    public void ClearAll()
    {
        fireTicks = electricTicks = 0;
        ResetVisuals();
    }
}
