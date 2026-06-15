using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Attack 7: Boss channels while shadow indicators grow at 3-5 random arena spots.
// After channeling, a horde enemy slams down on each spot one per beat.
public class BossHordeDropAttack : BossAttack
{
    [Header("Channel")]
    [SerializeField] int channelBeats = 4;

    [Header("Drop Sites")]
    [SerializeField] int minSites = 3;
    [SerializeField] int maxSites = 5;
    [SerializeField] float minDistFromPlayer = 3f;
    [SerializeField] float minDistBetweenSites = 2.5f;

    [Header("Arena Bounds")]
    [SerializeField] float arenaHalfX = 10f;
    [SerializeField] float arenaHalfY = 13f;

    [Header("Shadow")]
    [SerializeField] GameObject shadowPrefab;
    [SerializeField] float shadowMaxScale = 1.5f;

    [Header("Drop")]
    [SerializeField] GameObject mobEnemyPrefab;
    [SerializeField] GameObject impactVFXPrefab;
    [SerializeField] float impactVFXDuration = 0.5f;

    protected override bool CanActivateInternal(bool isPhase2) =>
        mobEnemyPrefab != null && player != null;

    protected override IEnumerator ExecuteAttack()
    {
        int siteCount = Random.Range(minSites, maxSites + 1);
        var positions = PickPositions(siteCount);

        float channelTime = channelBeats * Spb();

        var shadows = new List<Transform>();
        foreach (var pos in positions)
        {
            if (shadowPrefab == null) continue;
            var s = Instantiate(shadowPrefab, pos, Quaternion.identity).transform;
            s.localScale = Vector3.zero;
            shadows.Add(s);
            StartCoroutine(GrowShadow(s, channelTime));
        }

        yield return WaitBeats(channelBeats);

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];

            if (i < shadows.Count && shadows[i] != null)
                Destroy(shadows[i].gameObject);

            if (impactVFXPrefab != null)
            {
                var vfx = Instantiate(impactVFXPrefab, pos, Quaternion.identity);
                Destroy(vfx, impactVFXDuration);
            }

            if (mobEnemyPrefab != null)
                Instantiate(mobEnemyPrefab, pos, Quaternion.identity);

            if (i < positions.Count - 1)
                yield return WaitBeats(1f);
        }

        // Clean up shadows that didn't get destroyed (edge case: attack interrupted)
        foreach (var s in shadows)
            if (s != null) Destroy(s.gameObject);
    }

    IEnumerator GrowShadow(Transform shadow, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && shadow != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease in: slow at first, faster near landing — builds tension
            shadow.localScale = Vector3.one * (shadowMaxScale * Mathf.Sqrt(t));
            yield return null;
        }
        if (shadow != null)
            shadow.localScale = Vector3.one * shadowMaxScale;
    }

    List<Vector3> PickPositions(int count)
    {
        var result = new List<Vector3>();
        const int maxAttempts = 30;

        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var candidate = new Vector3(
                    Random.Range(-arenaHalfX, arenaHalfX),
                    Random.Range(-arenaHalfY, arenaHalfY),
                    0f);

                if (player != null && Vector3.Distance(candidate, player.position) < minDistFromPlayer)
                    continue;

                bool tooClose = false;
                foreach (var existing in result)
                    if (Vector3.Distance(candidate, existing) < minDistBetweenSites)
                    { tooClose = true; break; }

                if (!tooClose) { result.Add(candidate); break; }
            }
        }

        return result;
    }
}
