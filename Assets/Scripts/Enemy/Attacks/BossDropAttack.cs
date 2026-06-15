using UnityEngine;
using System.Collections;

// Attack 3: Boss channels (shows targeting reticle on player), then drops a
// lightning-style impact at the player's position. A mob enemy spawns 0.5 beats later.
public class BossDropAttack : BossAttack
{
    [Header("Channel")]
    [SerializeField] float channelBeats = 3f;
    [SerializeField] GameObject channelReticlePrefab;  // targeting circle shown on player during channel

    [Header("Drop")]
    [SerializeField] float damage = 20f;
    [SerializeField] float dropRadius = 1.5f;
    [SerializeField] GameObject dropVFXPrefab;         // visual for the drop zone (lightning hit)
    [SerializeField] float dropVFXDurationBeats = 5f;
    [SerializeField] GameObject mobEnemyPrefab;        // mob that spawns at drop position

    protected override bool CanActivateInternal(bool isPhase2) => false; // replaced by BossHordeDropAttack

    protected override IEnumerator ExecuteAttack()
    {
        // --- Channel phase: reticle follows player ---
        GameObject reticle = null;
        if (channelReticlePrefab != null)
            reticle = Instantiate(channelReticlePrefab, player.position, Quaternion.identity);

        float elapsed = 0f;
        float channelTime = channelBeats * Spb();
        while (elapsed < channelTime)
        {
            elapsed += Time.deltaTime;
            if (reticle != null) reticle.transform.position = player.position;
            yield return null;
        }

        if (reticle != null) Destroy(reticle);

        // --- Drop: hit player's current position ---
        Vector2 dropPos = player.position;

        // Immediate AOE damage
        var hits = Physics2D.OverlapCircleAll(dropPos, dropRadius);
        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
                col.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }

        // Drop VFX (auto-destroys after duration)
        if (dropVFXPrefab != null)
        {
            var vfx = Instantiate(dropVFXPrefab, (Vector3)(dropPos), Quaternion.identity);
            Destroy(vfx, dropVFXDurationBeats * Spb());
        }

        // TODO: drop audio cue (lightning strike sound)

        // --- Mob spawns 0.5 beats after drop ---
        yield return WaitBeats(0.5f);

        if (mobEnemyPrefab != null)
            Instantiate(mobEnemyPrefab, (Vector3)(dropPos), Quaternion.identity);
    }
}
