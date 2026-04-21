using UnityEngine;

public class BeatMarkerGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BeatmapData beatmap;
    [SerializeField] RectTransform markersParent;
    [SerializeField] RectTransform playhead;
    [SerializeField] GameObject beatMarkerPrefab;

    [Header("Settings")]
    [SerializeField] float barWidth = 200f;
    [SerializeField] int windowBeats = 4;

    [Header("Timing Windows")]
    [SerializeField] float perfectWindow = 0.05f;
    [SerializeField] float goodWindow = 0.1f;
    [SerializeField] float badWindow = 0.15f;

    [Header("Position")]
    [SerializeField] Vector3 defaultLocalOffset = new Vector3(0f, 1.5f, 0f);

    float windowSeconds;
    float pixelsPerSecond;
    Camera cam;
    RectTransform rectTransform;

    void Start()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        windowSeconds = windowBeats * beatmap.SecondsPerBeat;
        pixelsPerSecond = barWidth / windowSeconds;
        SpawnMarkers();
    }

    void SpawnMarkers()
    {
        for (int i = 0; i < windowBeats; i++)
        {
            float xPos = ((i + 0.5f) / windowBeats) * barWidth - barWidth / 2f;

            GameObject marker = Instantiate(beatMarkerPrefab, markersParent);
            RectTransform rt = marker.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(xPos, 0);

            SetZoneWidth(marker, "BadZone",     badWindow     * pixelsPerSecond * 2f);
            SetZoneWidth(marker, "GoodZone",    goodWindow    * pixelsPerSecond * 2f);
            SetZoneWidth(marker, "PerfectZone", perfectWindow * pixelsPerSecond * 2f);
        }
    }

    void SetZoneWidth(GameObject marker, string childName, float width)
    {
        Transform child = marker.transform.Find(childName);
        if (child == null) return;
        RectTransform rt = child.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
    }

    void Update()
    {
        if (BeatConductor.Instance == null) return;

        float posInWindow = Mathf.Repeat(BeatConductor.Instance.songPosition - beatmap.offset + 0.5f * beatmap.SecondsPerBeat, windowSeconds);
        float xPos = (posInWindow / windowSeconds) * barWidth - barWidth / 2f;
        playhead.anchoredPosition = new Vector2(xPos, playhead.anchoredPosition.y);
    }

    void LateUpdate()
    {
        float halfBarW = rectTransform.rect.width  * transform.lossyScale.x / 2f;
        float halfBarH = rectTransform.rect.height * transform.lossyScale.y / 2f;

        float halfCamH = cam.orthographicSize;
        float halfCamW = halfCamH * cam.aspect;

        Vector3 desired = transform.parent.position + defaultLocalOffset;
        Vector3 camPos = cam.transform.position;
        desired.x = Mathf.Clamp(desired.x, camPos.x - halfCamW + halfBarW, camPos.x + halfCamW - halfBarW);
        desired.y = Mathf.Clamp(desired.y, camPos.y - halfCamH + halfBarH, camPos.y + halfCamH - halfBarH);
        transform.position = desired;
    }
}
