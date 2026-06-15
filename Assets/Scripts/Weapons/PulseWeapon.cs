using UnityEngine;

public class PulseWeapon : Weapon
{
    [Header("Damage")]
    public float perfectDamage = 25f;
    public float goodDamage    = 18f;
    public float autoDamage    = 8f;

    [Header("Radius")]
    public float perfectRadius = 6f;
    public float goodRadius    = 4f;
    public float autoRadius    = 2.5f;

    [Header("Knockback")]
    public bool  applyKnockback      = false;
    public float knockbackDistance   = 2f;
    public float knockbackDuration   = 0.4f;

    [Header("Effects")]
    public GameObject impactEffectPrefab;
    public GameObject hitEffectPrefab;
    public float impactEffectDuration = 1f;
    public float hitEffectDuration    = 0.5f;

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? goodDamage    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage    + AutoDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Bad") return;

        float damage = judgement == "Perfect" ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus :
                       judgement == "Good"    ? goodDamage    + GoodDamageBonus    + ActiveDamageBonus :
                                               autoDamage     + AutoDamageBonus;

        float radius = judgement == "Perfect" ? perfectRadius :
                       judgement == "Good"    ? goodRadius : autoRadius;

        radius *= 1f + SizeBonus * 0.1f;

        float multiplier = (ComboSystem.Instance   != null ? ComboSystem.Instance.GetCurrentMultiplier()    : 1f)
                         * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f);
        float bonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;

        Vector2 center = (GetComponentInParent<PlayerMovement>()?.transform ?? transform).position;

        if (impactEffectPrefab != null)
        {
            GameObject fx = Instantiate(impactEffectPrefab, center, Quaternion.identity);
            SpriteRenderer sr = fx.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                float naturalRadius = sr.sprite.rect.width / sr.sprite.pixelsPerUnit * 0.5f;
                if (naturalRadius > 0f)
                    fx.transform.localScale = Vector3.one * (radius / naturalRadius);
            }
            Destroy(fx, impactEffectDuration);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage((damage + bonus) * multiplier, weaponType);
                ApplyDots(hit.gameObject);
            }

            if (hitEffectPrefab != null)
            {
                GameObject fx = Instantiate(hitEffectPrefab, hit.transform.position, Quaternion.identity);
                Destroy(fx, hitEffectDuration);
            }

            if (applyKnockback)
            {
                hit.GetComponent<MobEnemyMovement>()?.Push(center, knockbackDistance, knockbackDuration);
                hit.GetComponent<ExplodingEnemyMovement>()?.Push(center, knockbackDistance, knockbackDuration);
            }
        }
    }
}
