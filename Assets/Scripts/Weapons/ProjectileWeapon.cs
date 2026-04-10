using UnityEngine;

public class ProjectileWeapon : Weapon
{
    public GameObject projectilePrefab;
    public int projectileCount = 1;

    public float searchRadius = 10f;

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
            Vector2 dir = (target.position - transform.position).normalized;
            proj.GetComponent<Projectile>().SetDirection(dir);
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
