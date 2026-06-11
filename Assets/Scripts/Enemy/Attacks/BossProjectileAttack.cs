using UnityEngine;
using System.Collections;

// Attack 1: Fires a crescent/rectangle projectile at the player.
// Phase 2: fires 3 in a burst, spaced 1/4 beat apart.
public class BossProjectileAttack : BossAttack
{
    [Header("Projectile")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float projectileSpeed = 8f;
    [SerializeField] float damage = 10f;

    [Header("Phase 2 Burst")]
    [SerializeField] int phase2BurstCount = 3;

    protected override bool CanActivateInternal(bool isPhase2) =>
        projectilePrefab != null;

    protected override IEnumerator ExecuteAttack()
    {
        int shots = IsPhase2 ? phase2BurstCount : 1;

        for (int i = 0; i < shots; i++)
        {
            FireProjectile();
            if (i < shots - 1)
                yield return WaitBeats(0.25f);
        }
    }

    void FireProjectile()
    {
        if (player == null || projectilePrefab == null) return;

        Vector2 dir = (player.position - bossTransform.position).normalized;
        var go = Instantiate(projectilePrefab, bossTransform.position, Quaternion.identity);
        go.transform.right = dir;

        var proj = go.GetComponent<BossProjectile>();
        if (proj == null) return;

        proj.damage = damage;
        proj.Launch(dir, projectileSpeed);
    }
}
