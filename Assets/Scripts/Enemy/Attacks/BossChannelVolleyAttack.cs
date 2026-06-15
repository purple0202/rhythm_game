using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Attack 6: Boss channels, spawning projectiles that orbit in a circle (Zenyatta style).
// After channeling, fires them one per beat — each one parriable.
public class BossChannelVolleyAttack : BossAttack
{
    [Header("Volley")]
    [SerializeField] GameObject orbitalPrefab;    // small visual that orbits during channel
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float damage = 20f;
    [SerializeField] float parryDamage = 15f;
    [SerializeField] int channelBeats = 6;
    [SerializeField] float orbitRadius = 1.2f;
    [SerializeField] float orbitSpeed = 180f;     // degrees per second

    public override bool IsParryable => true;

    readonly List<GameObject> orbitals = new();

    protected override bool CanActivateInternal(bool isPhase2) =>
        projectilePrefab != null && player != null;

    protected override IEnumerator ExecuteAttack()
    {
        orbitals.Clear();

        // Announce all parry beats upfront. Anchor to lastBeatTime (exact beat boundary)
        // not songPosition (which can be mid-beat), so markers align with the beat grid.
        if (BeatMarkerLane.Instance != null && BeatConductor.Instance != null)
        {
            float anchorBeat = BeatConductor.Instance.lastBeatTime;
            for (int i = 0; i < channelBeats; i++)
                BeatMarkerLane.Instance.SpawnParryMarker(anchorBeat + (channelBeats + i + 2) * Spb());
        }

        // Channel phase: one orbital spawns per beat
        for (int i = 0; i < channelBeats; i++)
        {
            SpawnOrbital(i);
            yield return WaitBeats(1f);
        }

        // Fire phase: launch orbitals toward player, one per beat, each parriable
        int count = orbitals.Count;
        for (int i = 0; i < count; i++)
        {
            if (orbitals[i] != null)
                Destroy(orbitals[i]);

            if (player == null) continue;

            Vector2 dir = (player.position - bossTransform.position).normalized;
            float dist = Vector2.Distance(bossTransform.position, player.position);
            float speed = dist / Spb();

            var go = Instantiate(projectilePrefab, bossTransform.position, Quaternion.identity);
            go.transform.up = -dir;

            var proj = go.GetComponent<BossProjectile>();
            if (proj != null)
            {
                proj.damage = damage;
                proj.isParriable = true;
                proj.Launch(dir, speed);
                var bossHealth = bossTransform.GetComponent<EnemyHealth>();
                BossParryManager.Instance?.OpenWindow(proj, 1f, bossHealth, parryDamage);
            }

            // TODO: per-projectile "PARRY!" visual + audio notice

            yield return WaitBeats(1f);
        }

        // Clean up any orbitals that weren't launched (e.g. if player died mid-attack)
        foreach (var o in orbitals)
            if (o != null) Destroy(o);
        orbitals.Clear();
    }

    void OnDestroy()
    {
        foreach (var o in orbitals)
            if (o != null) Destroy(o);
        orbitals.Clear();
    }

    void SpawnOrbital(int index)
    {
        if (orbitalPrefab == null) return;
        var go = Instantiate(orbitalPrefab, bossTransform.position, Quaternion.identity);
        orbitals.Add(go);
        StartCoroutine(OrbitRoutine(go, index));
    }

    IEnumerator OrbitRoutine(GameObject orbital, int slotIndex)
    {
        while (orbital != null && bossTransform != null)
        {
            // Orbitals spread evenly and redistribute as more spawn
            int count = Mathf.Max(1, orbitals.Count);
            float baseAngle = 360f / count * slotIndex;
            float angle = baseAngle + orbitSpeed * Time.time;
            orbital.transform.position = bossTransform.position +
                Quaternion.Euler(0f, 0f, angle) * Vector3.right * orbitRadius;
            yield return null;
        }
    }
}
