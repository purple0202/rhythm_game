using System.Collections.Generic;
using UnityEngine;

public class BossLane : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject lanePanel;
    [SerializeField] RectTransform markersParent;   // center pivot, spans full lane width
    [SerializeField] RectTransform judgmentZone;    // positioned on the left side
    [SerializeField] GameObject chargeMarkerPrefab;
    [SerializeField] GameObject attackMarkerPrefab;

    [Header("Timing")]
    [SerializeField] float lookAheadBeats = 4f;
    [SerializeField] float destroyAfterBeats = 0.5f;

    readonly List<FlyingBeatMarker> activeMarkers = new();
    bool isActive;
    int absoluteBeat;
    int nextSpawnBeat;
    int patternLen;
    float judgmentX;
    float travelPixels;

    public static BossLane Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        lanePanel.SetActive(false);
    }

    void OnEnable() => BeatConductor.OnBeat += OnBeat;
    void OnDisable() => BeatConductor.OnBeat -= OnBeat;

    public void ShowLane(int patternLength)
    {
        absoluteBeat = 0;
        nextSpawnBeat = 1;
        patternLen = patternLength;

        judgmentX = markersParent.InverseTransformPoint(judgmentZone.position).x;
        travelPixels = markersParent.rect.xMax - judgmentX;

        ClearMarkers();
        isActive = true;
        lanePanel.SetActive(true);
    }

    public void HideLane()
    {
        isActive = false;
        ClearMarkers();
        lanePanel.SetActive(false);
    }

    public void SetPattern(int patternLength)
    {
        patternLen = patternLength;
        absoluteBeat = 0;
        nextSpawnBeat = 1;
        ClearMarkers();
    }

    void OnBeat()
    {
        if (isActive) absoluteBeat++;
    }

    void Update()
    {
        if (!isActive) return;

        var bc = BeatConductor.Instance;
        if (bc == null || bc.secondsPerBeat <= 0) return;

        float spb = bc.secondsPerBeat;
        float now = bc.songPosition;
        float lastBeat = bc.lastBeatTime;
        float pixelsPerSecond = travelPixels / (lookAheadBeats * spb);
        float destroyX = judgmentX - destroyAfterBeats * spb * pixelsPerSecond;

        while (true)
        {
            float beatTime = lastBeat + (nextSpawnBeat - absoluteBeat) * spb;
            if (beatTime > now + lookAheadBeats * spb) break;

            if (beatTime >= now - destroyAfterBeats * spb)
            {
                int patternPos = (nextSpawnBeat - 1) % patternLen;
                SpawnMarker(beatTime, patternPos == patternLen - 1, now, pixelsPerSecond);
            }

            nextSpawnBeat++;
        }

        for (int i = activeMarkers.Count - 1; i >= 0; i--)
        {
            var m = activeMarkers[i];
            if (m == null) { activeMarkers.RemoveAt(i); continue; }

            float x = judgmentX + (m.BeatTime - now) * pixelsPerSecond;
            m.SetX(x);

            if (x < destroyX)
            {
                Destroy(m.gameObject);
                activeMarkers.RemoveAt(i);
            }
        }
    }

    void SpawnMarker(float beatTime, bool isAttack, float now, float pixelsPerSecond)
    {
        var prefab = isAttack ? attackMarkerPrefab : chargeMarkerPrefab;
        if (prefab == null) return;

        var go = Instantiate(prefab, markersParent);
        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        }

        var marker = go.GetComponent<FlyingBeatMarker>();
        if (marker == null) marker = go.AddComponent<FlyingBeatMarker>();
        marker.Init(beatTime);
        marker.SetX(judgmentX + (beatTime - now) * pixelsPerSecond);

        activeMarkers.Add(marker);
    }

    void ClearMarkers()
    {
        foreach (var m in activeMarkers)
            if (m != null) Destroy(m.gameObject);
        activeMarkers.Clear();
    }

    public void ShowParryNotice()
    {
        // TODO: spawn a ZZZ-style parry warning marker at the next beat position in the lane
        Debug.Log("BossLane: Parry incoming!");
    }
}
