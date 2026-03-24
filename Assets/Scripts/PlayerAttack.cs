using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Damage Settings")]
    public float perfectDamage = 15f;
    public float goodDamage = 10f;

    [Header("Attack Settings")]
    public float attackRadius = 1.5f;
    public LayerMask enemyLayer;

    [Header("Visual")]
    public GameObject attackEffectPrefab;
    public float effectDuration = 0.2f;

    public void PerformAttack(string judgement)
    {
        float damage = 0f;

        if (judgement == "Perfect")
            damage = perfectDamage;
        else if (judgement == "Good")
            damage = goodDamage;
        else
            return; // no attack on bad/miss

        // Detect enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        // Spawn visual effect
        StartCoroutine(SpawnEffect());
    }

    IEnumerator SpawnEffect()
    {
        GameObject effect = Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);
        effect.transform.parent = transform;

        yield return new WaitForSeconds(effectDuration);

        Destroy(effect);
    }
}