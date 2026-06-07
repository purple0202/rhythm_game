using UnityEngine;
using System.Collections;

// Attack 4: Expands a damage ring from boss position across the screen.
// Applies slow or confusion on hit. Phase 2: applies both debuffs for longer.
public class BossRingPulseAttack : BossAttack
{
    [Header("Ring")]
    [SerializeField] GameObject ringPrefab;
    [SerializeField] float damage = 8f;
    [SerializeField] float ringSpeed = 6f;
    [SerializeField] float debuffDuration = 3f;

    [Header("Phase 2")]
    [SerializeField] float phase2DebuffDuration = 5f;

    protected override bool CanActivateInternal(bool isPhase2) => ringPrefab != null;

    protected override IEnumerator ExecuteAttack()
    {
        var go = Instantiate(ringPrefab, bossTransform.position, Quaternion.identity);
        var ring = go.GetComponent<BossRing>();
        if (ring != null)
        {
            float duration = IsPhase2 ? phase2DebuffDuration : debuffDuration;
            ring.Init(damage, ringSpeed, duration, IsPhase2);
        }

        // Ring manages its own lifetime; attack completes immediately and enters cooldown
        yield return null;
    }
}
