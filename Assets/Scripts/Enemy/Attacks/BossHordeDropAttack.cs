using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Attack 7: Boss channels while shadow + reticle indicators appear at 3-5 random arena spots.
// Shadow grows from zero over the channel. Reticle locks to the spot and slowly rotates.
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

    [Header("Screen Bounds")]
    [SerializeField] float screenEdgeMargin = 1f;

    [Header("Shadow")]
    [SerializeField] GameObject shadowPrefab;
    [SerializeField] float shadowMaxScale = 1.5f;

    [Header("Reticle")]
    [SerializeField] GameObject reticlePrefab;
    [SerializeField] float reticleRotateSpeed = 45f; // degrees per second

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

        var shadows  = new List<Transform>();
        var reticles = new List<Transform>();

        foreach (var pos in positions)
        {
            if (shadowPrefab != null)
            {
                var s = Instantiate(shadowPrefab, pos, Quaternion.identity).transform;
                s.localScale = Vector3.zero;
                var sr = s.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = 0;
                shadows.Add(s);
                StartCoroutine(GrowShadow(s, channelTime));
            }

            if (reticlePrefab != null)
            {
                var r = Instantiate(reticlePrefab, pos, Quaternion.identity).transform;
                var sr = r.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = 1;
                reticles.Add(r);
                StartCoroutine(SpinReticle(r));
            }
        }

        yield return WaitBeats(channelBeats);

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];

            if (i < shadows.Count  && shadows[i]  != null) Destroy(shadows[i].gameObject);
            if (i < reticles.Count && reticles[i] != null) Destroy(reticles[i].gameObject);

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

        foreach (var s in shadows)  if (s != null) Destroy(s.gameObject);
        foreach (var r in reticles) if (r != null) Destroy(r.gameObject);
    }

    IEnumerator GrowShadow(Transform shadow, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && shadow != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            shadow.localScale = Vector3.one * (shadowMaxScale * Mathf.Sqrt(t));
            yield return null;
        }
        if (shadow != null)
            shadow.localScale = Vector3.one * shadowMaxScale;
    }

    IEnumerator SpinReticle(Transform reticle)
    {
        while (reticle != null)
        {
            reticle.Rotate(0f, 0f, reticleRotateSpeed * Time.deltaTime);
            yield return null;
        }
    }

    List<Vector3> PickPositions(int count)
    {
        var result = new List<Vector3>();
        const int maxAttempts = 30;

        Camera cam = Camera.main;
        float halfH = cam.orthographicSize - screenEdgeMargin;
        float halfW = halfH * cam.aspect;
        Vector3 camPos = cam.transform.position;

        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var candidate = new Vector3(
                    camPos.x + Random.Range(-halfW, halfW),
                    camPos.y + Random.Range(-halfH, halfH),
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
