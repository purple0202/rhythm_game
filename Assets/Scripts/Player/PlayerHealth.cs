using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerStats player_stats;
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Damage Settings")]
    public float damageCooldown = 0.5f;

    float lastDamageTime;

    bool isInvincible = false;

    public HealthBar healthBar;

    void Start()
    {
        maxHealth = player_stats.maxHealth;
        currentHealth = maxHealth;
        
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible) return;
        // Cooldown check
        if (Time.time < lastDamageTime + damageCooldown)
            return;

        lastDamageTime = Time.time;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player Health: " + currentHealth);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
    void Die()
    {
        Debug.Log("Player Died");
        // Later: game over logic
    }
}