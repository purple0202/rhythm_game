using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FlyingBeatMarker : MonoBehaviour
{
    float beatTime;
    public float BeatTime => beatTime;
    public bool  IsParry  { get; private set; }

    RectTransform rt;

    void Awake() => rt = GetComponent<RectTransform>();

    public void Init(float bt, bool isParry = false)
    {
        beatTime = bt;
        IsParry  = isParry;
    }

    public void SetX(float x)
    {
        Vector2 pos = rt.anchoredPosition;
        pos.x = x;
        rt.anchoredPosition = pos;
    }
}
