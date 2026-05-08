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
    public override bool IsAttacking => transform.childCount > 0;

    Vector3 gizmoCenter;
    float gizmoRadius;

    public override void PerformAttack(string judgement)
    {
        float damage;

        if (judgement == "Perfect")
            damage = perfectDamage;
        else if (judgement == "Good")
            damage = goodDamage;
        else if (judgement == "Auto")
            damage = autoDamage;
        else
            return; // "Bad"

        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();
        int facing = playerMovement != null ? playerMovement.facingDirection : 1;
        Vector3 spawnPos = transform.position + new Vector3(effectOffset * facing, 0f, 0f);

        GameObject effect = Instantiate(attackEffectPrefab, spawnPos, Quaternion.identity, transform);
        if (facing == -1)
        {
            Vector3 scale = effect.transform.localScale;
            scale.x *= -1f;
            effect.transform.localScale = scale;
        }

        float radius = attackRadius;
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

        Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPos, radius, enemyLayer);
        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage * multiplier, weaponType);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawSphere(gizmoCenter, gizmoRadius);
    }
}
