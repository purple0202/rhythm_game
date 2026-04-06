using UnityEngine;
using System.Collections;

public class MeleeWeapon : Weapon
{
    public float perfectDamage = 15f;
    public float goodDamage = 10f;

    public float attackRadius = 1.5f;
    public LayerMask enemyLayer;

    public GameObject attackEffectPrefab;
    public float effectDuration = 0.2f;

    public override void PerformAttack(string judgement)
    {
        float damage = 0f;

        if (judgement == "Perfect")
            damage = perfectDamage;
        else if (judgement == "Good")
            damage = goodDamage;
        else
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRadius,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            if (enemy != null)
                enemy.TakeDamage(damage);
        }

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