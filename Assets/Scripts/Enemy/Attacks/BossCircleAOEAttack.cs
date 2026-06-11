using UnityEngine;
using System.Collections;

// Attack 2: Forecast circles march from boss toward player at fixed spacing,
// each one exploding before the next appears.
// Phase 2: forecast time halved, circles grow larger with each step.
public class BossCircleAOEAttack : BossAttack
{
    [Header("Circle AOE")]
    [SerializeField] GameObject circleIndicatorPrefab;
    [SerializeField] float damage = 15f;
    [SerializeField] float circleRadius = 1.5f;
    [SerializeField] int circleCount = 5;
    [SerializeField] float stepDistance = 2f;   // fixed spacing between circles
    [SerializeField] float forecastBeats = 0.5f;

    [Header("Phase 2")]
    [SerializeField] float phase2ForecastBeats = 0.25f;
    [SerializeField] float phase2RadiusGrowthPerStep = 0.35f;

    protected override bool CanActivateInternal(bool isPhase2) =>
        circleIndicatorPrefab != null;

    protected override IEnumerator ExecuteAttack()
    {
        float forecast = IsPhase2 ? phase2ForecastBeats : forecastBeats;
        Vector2 bossPos = bossTransform.position;
        Vector2 dir = ((Vector2)player.position - bossPos).normalized;

        for (int i = 0; i < circleCount; i++)
        {
            float radius = circleRadius + (IsPhase2 ? i * phase2RadiusGrowthPerStep : 0f);
            Vector2 pos = bossPos + dir * (stepDistance * (i + 1));
            SpawnCircle(pos, radius, forecast);
            yield return WaitBeats(forecast);  // previous circle explodes, next one spawns
        }

        // Wait for the last circle to finish before cooldown starts
        yield return WaitBeats(forecast);
    }

    void SpawnCircle(Vector2 pos, float radius, float forecast)
    {
        var go = Instantiate(circleIndicatorPrefab, (Vector3)(Vector2)pos, Quaternion.identity);
        var indicator = go.GetComponent<BossCircleIndicator>();
        if (indicator != null)
        {
            indicator.Init(damage, radius);
            indicator.Begin(forecast, Spb());
        }
    }
}
