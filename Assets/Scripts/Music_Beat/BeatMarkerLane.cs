using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Taiko-style beat lane: a fixed judgment zone on the left, circular markers
/// spawn off the right edge and fly left. Position is computed purely from
/// (beatTime - songPosition) * pixelsPerSecond, so accuracy never drifts.
/// </summary>
public class BeatMarkerLane : MonoBehaviour
{
    public static BeatMarkerLane Instance { get; private set; }

    [Header("References")]
    [SerializeField] BeatmapData beatmap;
    [SerializeField] RectTransform markersParent;
    [SerializeField] RectTransform judgmentZone;
    [SerializeField] GameObject markerPrefab;
    [SerializeField] GameObject parryMarkerPrefab;

    [Header("Timing")]
    [Tooltip("How many beats ahead of the judgment zone markers are visible.")]
    [SerializeField] float lookAheadBeats = 3f;
    [Tooltip("How many beats past the judgment zone before a marker is destroyed.")]
    [SerializeField] float destroyAfterBeats = 0.5f;

    [Header("Follow (World Space Canvas)")]
    [Tooltip("Offset from the parent (player) in world units.")]
    [SerializeField] Vector3 defaultLocalOffset = new Vector3(0f, 1.5f, 0f);

    readonly List<FlyingBeatMarker> activeMarkers = new();
    int nextBeatIndex;
    float judgmentX;
    float travelPixels;
    float previousSongPosition;

    Camera cam;
    RectTransform rt;

    void Awake() => Instance = this;

    void Start()
    {
        cam = Camera.main;
        rt = GetComponent<RectTransform>();

        // Convert the judgment zone's world position into markersParent's local space.
        // This is anchor-independent — it works no matter how JudgmentZone is anchored.
        judgmentX = markersParent.InverseTransformPoint(judgmentZone.position).x;

        // Right edge of the lane in the same local space, so markers spawn exactly there.
        travelPixels = rt.rect.width / 2f - judgmentX;

        float spb = beatmap.SecondsPerBeat;
        float now = BeatConductor.Instance.songPosition;
        previousSongPosition = now;
        nextBeatIndex = Mathf.Max(0, Mathf.FloorToInt((now - beatmap.offset) / spb));
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        float spb = BeatConductor.Instance.secondsPerBeat > 0
            ? BeatConductor.Instance.secondsPerBeat
            : beatmap.SecondsPerBeat;

        float now = BeatConductor.Instance.songPosition;

        // If we're returning from a freeze, now will be ahead of previousSongPosition
        // by the full pause duration. Resync: clear stale regular markers and snap
        // nextBeatIndex forward so the spawn loop doesn't iterate through every missed beat.
        if (now > previousSongPosition + spb)
        {
            for (int i = activeMarkers.Count - 1; i >= 0; i--)
            {
                var m = activeMarkers[i];
                if (m == null) { activeMarkers.RemoveAt(i); continue; }
                if (m.IsParry) continue;
                Destroy(m.gameObject);
                activeMarkers.RemoveAt(i);
            }
            nextBeatIndex = Mathf.Max(nextBeatIndex, Mathf.FloorToInt((now - beatmap.offset) / spb));
        }

        // If song position jumped backward by more than half a beat, the track looped.
        // Clear all markers except parry markers still waiting to hit the judgment zone.
        if (now < previousSongPosition - spb * 0.5f)
        {
            for (int i = activeMarkers.Count - 1; i >= 0; i--)
            {
                var m = activeMarkers[i];
                if (m == null) { activeMarkers.RemoveAt(i); continue; }
                if (m.IsParry && m.BeatTime > now) continue;
                Destroy(m.gameObject);
                activeMarkers.RemoveAt(i);
            }
            nextBeatIndex = Mathf.Max(0, Mathf.FloorToInt((now - beatmap.offset) / spb));
        }
        previousSongPosition = now;
        float pixelsPerSecond = travelPixels / (lookAheadBeats * spb);
        float destroyX = judgmentX - destroyAfterBeats * spb * pixelsPerSecond;

        // Spawn markers that have entered the look-ahead window.
        float spawnHorizon = now + lookAheadBeats * spb;
        while (true)
        {
            float beatTime = beatmap.offset + nextBeatIndex * spb;
            if (beatTime > spawnHorizon) break;

            // Skip any that are already past the destroy threshold.
            if (beatTime >= now - destroyAfterBeats * spb)
                Spawn(beatTime, now, pixelsPerSecond);

            nextBeatIndex++;
        }

        // Update positions and cull markers that have passed the destroy threshold.
        for (int i = activeMarkers.Count - 1; i >= 0; i--)
        {
            FlyingBeatMarker m = activeMarkers[i];
            if (m == null) { activeMarkers.RemoveAt(i); continue; }

            float x = judgmentX + (m.BeatTime - now) * pixelsPerSecond;
            m.SetX(x);

            // Parry markers are never destroyed before their beat time arrives
            bool canDestroy = !m.IsParry || m.BeatTime <= now;
            if (x < destroyX && canDestroy)
            {
                Destroy(m.gameObject);
                activeMarkers.RemoveAt(i);
            }
        }
    }

    void LateUpdate()
    {
        float halfW = rt.rect.width  * transform.lossyScale.x / 2f;
        float halfH = rt.rect.height * transform.lossyScale.y / 2f;

        float halfCamH = cam.orthographicSize;
        float halfCamW = halfCamH * cam.aspect;

        Vector3 desired = transform.parent.position + defaultLocalOffset;
        Vector3 camPos  = cam.transform.position;
        desired.x = Mathf.Clamp(desired.x, camPos.x - halfCamW + halfW, camPos.x + halfCamW - halfW);
        desired.y = Mathf.Clamp(desired.y, camPos.y - halfCamH + halfH, camPos.y + halfCamH - halfH);
        transform.position = desired;
    }

    public void SpawnParryMarker(float songPositionAtImpact)
    {
        if (parryMarkerPrefab == null) return;
        float spb = BeatConductor.Instance.secondsPerBeat > 0
            ? BeatConductor.Instance.secondsPerBeat
            : beatmap.SecondsPerBeat;
        float pixelsPerSecond = travelPixels / (lookAheadBeats * spb);
        float now = BeatConductor.Instance.songPosition;
        Spawn(parryMarkerPrefab, songPositionAtImpact, now, pixelsPerSecond, isParry: true);
    }

    void Spawn(float beatTime, float now, float pixelsPerSecond)
        => Spawn(markerPrefab, beatTime, now, pixelsPerSecond);

    void Spawn(GameObject prefab, float beatTime, float now, float pixelsPerSecond, bool isParry = false)
    {
        GameObject go = Instantiate(prefab, markersParent);

        RectTransform markerRt = go.GetComponent<RectTransform>();
        markerRt.anchorMin = new Vector2(0.5f, 0.5f);
        markerRt.anchorMax = new Vector2(0.5f, 0.5f);
        markerRt.pivot     = new Vector2(0.5f, 0.5f);

        FlyingBeatMarker marker = go.GetComponent<FlyingBeatMarker>();
        marker.Init(beatTime, isParry);
        marker.SetX(judgmentX + (beatTime - now) * pixelsPerSecond);
        activeMarkers.Add(marker);
    }
}
