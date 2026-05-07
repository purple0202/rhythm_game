using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FlyingBeatMarker : MonoBehaviour
{
    public float BeatTime { get; private set; }

    RectTransform rt;

    void Awake() => rt = GetComponent<RectTransform>();

    public void Init(float beatTime) => BeatTime = beatTime;

    public void SetX(float x)
    {
        Vector2 pos = rt.anchoredPosition;
        pos.x = x;
        rt.anchoredPosition = pos;
    }
}
