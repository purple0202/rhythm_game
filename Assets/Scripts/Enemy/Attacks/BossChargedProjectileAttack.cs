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

    [Header("Charge Visual")]
    [SerializeField] Transform chargeVisual;
    [SerializeField] float chargeMinScale = 0.3f;
    [SerializeField] float chargeMaxScale = 1f;
    [SerializeField] Animator chargeAnimator;
    [SerializeField] SpriteRenderer chargeRenderer;
    [SerializeField] Sprite fireFrameSprite;

    public override bool IsParryable => true;

    protected override bool CanActivateInternal(bool isPhase2) =>
        projectilePrefab != null && player != null;

    protected override IEnumerator ExecuteAttack()
    {
        // Announce to player lane at charge start so the marker travels the full visible window.
        // Use lastBeatTime (exact beat boundary) not songPosition (can be mid-beat).
        if (BeatMarkerLane.Instance != null && BeatConductor.Instance != null)
            BeatMarkerLane.Instance.SpawnParryMarker(BeatConductor.Instance.lastBeatTime + (chargeBeats + 2) * Spb());

        if (chargeVisual != null)
            chargeVisual.localScale = Vector3.zero;

        if (chargeAnimator != null)
        {
            chargeAnimator.enabled = true;
            chargeAnimator.Play(0, -1, 0f);
        }

        // Snap to a larger scale on each beat
        for (int i = 0; i < chargeBeats; i++)
        {
            float t = (i + 1f) / chargeBeats;
            if (chargeVisual != null)
                chargeVisual.localScale = Vector3.one * Mathf.Lerp(chargeMinScale, chargeMaxScale, t);
            yield return WaitBeats(1f);
        }

        // Switch to fire-flash frame
        if (chargeAnimator != null) chargeAnimator.enabled = false;
        if (chargeRenderer != null && fireFrameSprite != null)
            chargeRenderer.sprite = fireFrameSprite;

        if (player == null || projectilePrefab == null)
        {
            if (chargeVisual != null) chargeVisual.localScale = Vector3.zero;
            yield break;
        }

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

        // Hold fire-flash for 1/4 beat, then hide
        yield return WaitBeats(0.25f);
        if (chargeVisual != null) chargeVisual.localScale = Vector3.zero;

        // Wait remaining travel time before attack is considered complete (total = 1 beat)
        yield return WaitBeats(0.75f);
    }
}
