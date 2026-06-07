using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 20f;
    private float currentHealth;
    public float CurrentHealth => currentHealth;
    public System.Action onDeath;
    public int expValue = 10;

    [Header("Type")]
    public EnemyType enemyType = EnemyType.None;

    [Header("Outline Materials")]
    public Material blueMaterial;
    public Material redMaterial;
    public Material greenMaterial;
    public Material yellowMaterial;

    [Header("Damage Popup")]
    [SerializeField] GameObject damagePopupPrefab;

    // Damage multipliers
    private const float MatchMultiplier    = 2.0f;
    private const float MismatchMultiplier = 0.5f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        EnemyManager.Instance?.RegisterEnemy(gameObject);

        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyOutline();
    }

    public void SetType(EnemyType type)
    {
        enemyType = type;
        ApplyOutline();
    }

    public void TakeDamage(float damage, EnemyType weaponType = EnemyType.None, Color? popupColor = null, Vector2 popupOffset = default)
    {
        float multiplier = GetDamageMultiplier(weaponType);
        float trueDamage = damage * multiplier;
        currentHealth -= trueDamage;

        if (damagePopupPrefab != null)
        {
            Vector3 spawnPos = transform.position + (Vector3)popupOffset;
            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
            var dp = popup.GetComponent<DamagePopup>();
            if (popupColor.HasValue)
                dp.Setup(trueDamage, popupColor.Value);
            else
                dp.Setup(trueDamage);
        }

        if (currentHealth <= 0)
            Die();
    }

    float GetDamageMultiplier(EnemyType weaponType)
    {
        // None-type enemies take normal damage from everything
        if (enemyType == EnemyType.None) return 1f;

        // None-type weapons deal normal damage to everything
        if (weaponType == EnemyType.None) return 1f;

        return weaponType == enemyType ? MatchMultiplier : MismatchMultiplier;
    }

    void ApplyOutline()
    {
        SpriteOutline outline = GetComponent<SpriteOutline>();
        if (outline != null)
        {
            Material mat = enemyType switch
            {
                EnemyType.Blue   => blueMaterial,
                EnemyType.Red    => redMaterial,
                EnemyType.Green  => greenMaterial,
                EnemyType.Yellow => yellowMaterial,
                _                => null
            };
            if (mat != null)
                outline.SetColor(mat.GetColor("_OutlineColor"));
            return;
        }

        // Legacy: material-based outline (rollback path)
        if (spriteRenderer == null) return;
        Material legacyMat = enemyType switch
        {
            EnemyType.Blue   => blueMaterial,
            EnemyType.Red    => redMaterial,
            EnemyType.Green  => greenMaterial,
            EnemyType.Yellow => yellowMaterial,
            _                => null
        };
        if (legacyMat != null)
            spriteRenderer.material = legacyMat;
    }

    void Die()
    {
        onDeath?.Invoke();
        GetComponent<DotReceiver>()?.ClearAll();
        FindObjectOfType<LevelSystem>().AddExp(expValue);
        EnemyManager.Instance.UnregisterEnemy(gameObject);

        ExplodingEnemyMovement exploding = GetComponent<ExplodingEnemyMovement>();
        if (exploding != null)
        {
            exploding.OnDeathExplosion();
            return; // exploding enemy destroys itself after the animation finishes
        }

        Destroy(gameObject);
    }
}
