using UnityEngine;

/// <summary>
/// Punches the judgment zone's scale on every beat as a visual cue.
/// Attach directly to the JudgmentZone GameObject.
/// </summary>
public class JudgmentZonePulse : MonoBehaviour
{
    [SerializeField] float punchScale = 1.25f;
    [SerializeField] float punchDuration = 0.12f;

    Vector3 baseScale;
    float punchTimer = -1f;

    void Awake() => baseScale = transform.localScale;
    void OnEnable()  => BeatConductor.OnBeat += Pulse;
    void OnDisable() => BeatConductor.OnBeat -= Pulse;

    void Pulse() => punchTimer = punchDuration;

    void Update()
    {
        if (punchTimer < 0f) return;

        punchTimer -= Time.deltaTime;
        float t = Mathf.Clamp01(punchTimer / punchDuration);
        transform.localScale = baseScale * Mathf.Lerp(1f, punchScale, t);
    }
}
