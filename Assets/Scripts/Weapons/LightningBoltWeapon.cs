using UnityEngine;

public class LightningBoltWeapon : Weapon
{
    [Header("Primary Damage")]
    public float perfectDamage = 18f;
    public float goodDamage    = 12f;
    public float autoDamage    = 6f;

    [Header("Spread Damage")]
    public float spreadDamage     = 8f;
    public float autoSpreadDamage = 4f;

    [Header("Spread Radius")]
    public float baseSpreadRadius    = 4f;
    public float perfectSpreadBonus  = 2f;
    public float goodSpreadBonus     = 1f;

    [Header("Search")]
    public float searchRadius = 20f;
    public LayerMask enemyLayer;

    [Header("Effects (optional)")]
    public GameObject primaryEffectPrefab;
    public GameObject spreadEffectPrefab;
    public float effectDuration = 0.3f;

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? goodDamage    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage    + AutoDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Bad") return;

        float sizeMult   = 1f + SizeBonus * 0.1f;
        float primaryDmg = GetDamageForJudgement(judgement);
        float sRadius    = judgement == "Perfect" ? (baseSpreadRadius + perfectSpreadBonus) * sizeMult
                         : judgement == "Good"    ? (baseSpreadRadius + goodSpreadBonus)    * sizeMult
                                                  :  baseSpreadRadius                       * sizeMult;

        float mult  = (ComboSystem.Instance    != null ? ComboSystem.Instance.GetCurrentMultiplier()    : 1f)
                    * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier()  : 1f);
        float bonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;

        Transform primaryTarget = FindNearestEnemy();
        if (primaryTarget == null) return;

        // Primary hit
        EnemyHealth primaryHealth = primaryTarget.GetComponent<EnemyHealth>();
        if (primaryHealth != null)
        {
            primaryHealth.TakeDamage((primaryDmg + bonus) * mult, weaponType);
            ApplyDots(primaryTarget.gameObject);
        }

        SpawnEffect(primaryEffectPrefab, primaryTarget.position);

        // Spread from primary enemy's position to all other enemies in radius
        float sDmg = (judgement == "Auto"
            ? autoSpreadDamage + AutoDamageBonus
            : spreadDamage     + ActiveDamageBonus) * mult;

        Collider2D[] hits = Physics2D.OverlapCircleAll(primaryTarget.position, sRadius, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.gameObject == primaryTarget.gameObject) continue;

            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            if (eh == null) continue;

            eh.TakeDamage(sDmg, weaponType);
            ApplyDots(hit.gameObject);
            SpawnEffect(spreadEffectPrefab, hit.transform.position);
        }
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var hit in hits)
        {
            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < minDist) { minDist = d; closest = hit.transform; }
        }
        return closest;
    }

    void SpawnEffect(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        Destroy(Instantiate(prefab, position, Quaternion.identity), effectDuration);
    }
}
