using UnityEngine;

public class ProjectileWeapon : Weapon
{
    public GameObject projectilePrefab;
    public int projectileCount = 1;
    public float autoDamage = 5f;

    public float searchRadius = 10f;

    public override float GetAutoDamage() => autoDamage;
    public override float GetDamageForJudgement(string j) =>
        j == "Bad" ? 0f : projectilePrefab != null ? projectilePrefab.GetComponent<Projectile>()?.damage ?? 0f : 0f;

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Bad") return;

        // Auto fires 1 projectile regardless of projectileCount
        int count = (judgement == "Auto") ? 1 : projectileCount;

        Transform target = FindClosestEnemy();
        if (target == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            p.weaponType = weaponType;
            p.damage += PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;
            p.SetDirection((target.position - transform.position).normalized);
        }
    }

    Transform FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius);

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }

        return closest;
    }
}
