using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 20f;
    private float currentHealth;
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

    void Start()
    {
        EnemyManager.Instance.RegisterEnemy(gameObject);
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyOutline();
    }

    public void SetType(EnemyType type)
    {
        enemyType = type;
        ApplyOutline();
    }

    public void TakeDamage(float damage, EnemyType weaponType = EnemyType.None)
    {
        float multiplier = GetDamageMultiplier(weaponType);
        float trueDamage = damage * multiplier;
        currentHealth -= trueDamage;

        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(trueDamage);
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
        if (spriteRenderer == null) return;

        Material mat = enemyType switch
        {
            EnemyType.Blue   => blueMaterial,
            EnemyType.Red    => redMaterial,
            EnemyType.Green  => greenMaterial,
            EnemyType.Yellow => yellowMaterial,
            _                => null
        };

        if (mat != null)
            spriteRenderer.material = mat;
    }

    void Die()
    {
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
