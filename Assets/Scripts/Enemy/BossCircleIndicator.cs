using UnityEngine;
using System.Collections;

// Spawned by BossCircleAOEAttack. Shows a forecast circle that fills like a clock,
// then spawns a separate explosion effect. The indicator itself does no damage.
public class BossCircleIndicator : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] Color outlineColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    [SerializeField] Color fillColor    = new Color(1f, 0.2f, 0.2f, 0.35f);
    [SerializeField] float outlineWidth = 0.08f;
    [SerializeField] float fillWidth    = 0.3f;
    [SerializeField] int segments = 64;

    [Header("Explosion")]
    [SerializeField] GameObject explosionPrefab;  // assign BossExplosionPrefab here
    [SerializeField] float lingerDuration = 0.4f; // how long the indicator stays after triggering

    float damage;
    float radius;

    LineRenderer outlineLine;
    LineRenderer fillLine;

    public void Init(float damage, float radius)
    {
        this.damage = damage;
        this.radius = radius;
        BuildOutline();
        BuildFillArc();
    }

    public void Begin(float forecastBeats, float spb)
    {
        StartCoroutine(ForecastAndExplode(forecastBeats * spb));
    }

    // --- Visual setup ---

    void BuildOutline()
    {
        var go = new GameObject("Outline");
        go.transform.SetParent(transform, false);

        outlineLine = go.AddComponent<LineRenderer>();
        outlineLine.useWorldSpace = false;
        outlineLine.loop = true;
        outlineLine.positionCount = segments;
        outlineLine.startWidth = outlineWidth;
        outlineLine.endWidth = outlineWidth;
        outlineLine.startColor = outlineColor;
        outlineLine.endColor = outlineColor;
        outlineLine.material = new Material(Shader.Find("Sprites/Default"));
        outlineLine.sortingOrder = 4;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            outlineLine.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius, 0f));
        }
    }

    void BuildFillArc()
    {
        var go = new GameObject("Fill");
        go.transform.SetParent(transform, false);

        fillLine = go.AddComponent<LineRenderer>();
        fillLine.useWorldSpace = false;
        fillLine.loop = false;
        fillLine.positionCount = 0;
        fillLine.startWidth = fillWidth;
        fillLine.endWidth = fillWidth;
        fillLine.startColor = fillColor;
        fillLine.endColor = fillColor;
        fillLine.material = new Material(Shader.Find("Sprites/Default"));
        fillLine.sortingOrder = 3;
    }

    void UpdateFillArc(float t)
    {
        int count = Mathf.Max(2, Mathf.RoundToInt(t * segments));
        fillLine.positionCount = count;

        float innerRadius = radius - fillWidth * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float angle = (float)i / (segments - 1) * Mathf.PI * 2f * t;
            fillLine.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * innerRadius,
                Mathf.Sin(angle) * innerRadius, 0f));
        }
    }

    // --- Forecast + trigger ---

    IEnumerator ForecastAndExplode(float forecastTime)
    {
        float elapsed = 0f;
        while (elapsed < forecastTime)
        {
            elapsed += Time.deltaTime;
            UpdateFillArc(Mathf.Clamp01(elapsed / forecastTime));
            yield return null;
        }

        TriggerExplosion();

        // Indicator lingers briefly after triggering, then disappears
        yield return new WaitForSeconds(lingerDuration);
        Destroy(gameObject);
    }

    void TriggerExplosion()
    {
        if (explosionPrefab == null) return;
        var go = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        go.GetComponent<BossExplosion>()?.Init(damage, radius);
    }
}
