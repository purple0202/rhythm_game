using UnityEngine;
using System.Collections;

// Attack 5: Boss charges up over several beats, then fires a high-damage projectile.
// Speed auto-calculated so it arrives exactly 1 beat after firing — the parry window.
public class BossChargedProjectileAttack : BossAttack
{
    [Header("Charged Projectile")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float damage = 35f;
    [SerializeField] float parryDamage = 25f;
    [SerializeField] int chargeBeats = 3;
    [SerializeField] Transform chargeVisual;   // child object that scales up during charge

    public override bool IsParryable => true;

    protected override bool CanActivateInternal(bool isPhase2) =>
        projectilePrefab != null && player != null;

    protected override IEnumerator ExecuteAttack()
    {
        // Charge up — visual grows each beat
        for (int i = 0; i < chargeBeats; i++)
        {
            float t = (i + 1f) / chargeBeats;
            if (chargeVisual != null)
                chargeVisual.localScale = Vector3.one * t;
            yield return WaitBeats(1f);
        }

        if (chargeVisual != null)
            chargeVisual.localScale = Vector3.zero;

        if (player == null || projectilePrefab == null) yield break;

        // Fire — speed calculated so it reaches the player in exactly 1 beat
        Vector2 dir = (player.position - bossTransform.position).normalized;
        float dist = Vector2.Distance(bossTransform.position, player.position);
        float speed = dist / Spb();

        var go = Instantiate(projectilePrefab, bossTransform.position, Quaternion.identity);
        go.transform.right = dir;

        var proj = go.GetComponent<BossProjectile>();
        if (proj != null)
        {
            proj.damage = damage;
            proj.isParriable = true;
            proj.Launch(dir, speed);
            var bossHealth = bossTransform.GetComponent<EnemyHealth>();
            BossParryManager.Instance?.OpenWindow(proj, 1f, bossHealth, parryDamage);
        }

        // TODO: "PARRY!" visual + audio notice

        // Wait 1 beat (travel time) before attack is considered complete
        yield return WaitBeats(1f);
    }
}
