using UnityEngine;

public class MeleeWeapon : Weapon
{
    public float perfectDamage = 15f;
    public float goodDamage = 10f;

    public float attackRadius = 1.5f;
    public bool useEffectCollider = true;
    public LayerMask enemyLayer;

    public GameObject attackEffectPrefab;
    public float effectOffset = 0.7f;

    public float autoDamage = 5f;

    // True while a SlashEffect child is alive (parented to this weapon's transform).
    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? goodDamage    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage    + AutoDamageBonus : 0f;
    public override bool IsAttacking => transform.childCount > 0;

    Vector3 gizmoCenter;
    float gizmoRadius;

    public override void PerformAttack(string judgement)
    {
        float damage;

        if (judgement == "Perfect")
            damage = perfectDamage + PerfectDamageBonus + ActiveDamageBonus;
        else if (judgement == "Good")
            damage = goodDamage + GoodDamageBonus + ActiveDamageBonus;
        else if (judgement == "Auto")
            damage = autoDamage + AutoDamageBonus;
        else
            return; // "Bad"

        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();
        int facing = playerMovement != null ? playerMovement.facingDirection : 1;

        // Shift spawn point outward and scale the visual with size bonus.
        float sizeMult = 1f + SizeBonus * 0.1f;
        Vector3 spawnPos = transform.position + new Vector3(effectOffset * facing * sizeMult, 0f, 0f);

        GameObject effect = Instantiate(attackEffectPrefab, spawnPos, Quaternion.identity, transform);
        Vector3 effectScale = Vector3.one * sizeMult;
        if (facing == -1) effectScale.x *= -1f;
        effect.transform.localScale = effectScale;

        // col.bounds already reflects the scaled transform, so no extra multiply needed.
        float radius = attackRadius * sizeMult;
        if (useEffectCollider)
        {
            CircleCollider2D col = effect.GetComponent<CircleCollider2D>();
            if (col != null)
                radius = col.bounds.extents.x;
        }

        gizmoCenter = spawnPos;
        gizmoRadius = radius;

        float multiplier = (ComboSystem.Instance != null ? ComboSystem.Instance.GetCurrentMultiplier() : 1f)
                         * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f);
        float bonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPos, radius, enemyLayer);
        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage((damage + bonus) * multiplier, weaponType);
                ApplyDots(hit.gameObject);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawSphere(gizmoCenter, gizmoRadius);
    }
}
