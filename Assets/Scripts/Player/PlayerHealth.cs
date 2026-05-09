using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerStats player_stats;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Shield")]
    public float currentShield;

    [Header("Damage Settings")]
    public float damageCooldown = 0.5f;

    float lastDamageTime;
    bool isInvincible = false;

    public HealthBar healthBar;

    public static event System.Action<float, float> OnShieldChanged;
    public static event System.Action               OnDamageTaken;

    void Start()
    {
        maxHealth = player_stats.maxHealth;
        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        OnShieldChanged?.Invoke(0f, maxHealth);
    }

    public void AddShield(float amount)
    {
        currentShield = Mathf.Clamp(currentShield + amount, 0f, maxHealth);
        OnShieldChanged?.Invoke(currentShield, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible) return;
        if (Time.time < lastDamageTime + damageCooldown) return;

        lastDamageTime = Time.time;

        if (currentShield > 0f)
        {
            float absorbed = Mathf.Min(currentShield, amount);
            currentShield -= absorbed;
            amount -= absorbed;
            OnShieldChanged?.Invoke(currentShield, maxHealth);
        }

        if (amount <= 0f) return;

        OnDamageTaken?.Invoke();
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0f)
            Die();
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    void Die()
    {
        Debug.Log("Player Died");
    }
}
