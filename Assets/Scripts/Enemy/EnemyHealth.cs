using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 20f;
    private float currentHealth;
    public int expValue = 10;

    void Start()
    {
        EnemyManager.Instance.RegisterEnemy(gameObject);
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        FindObjectOfType<LevelSystem>().AddExp(expValue);
        EnemyManager.Instance.UnregisterEnemy(gameObject);
        Destroy(gameObject);

    }
}