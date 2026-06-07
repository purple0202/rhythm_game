using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections.Generic;

public class PuppetBossIntroUI : MonoBehaviour
{
    public static PuppetBossIntroUI Instance;

    [System.Serializable]
    public class PropConfig
    {
        public Sprite sprite;
        public Vector2 pivotPos;
        public float ropeLength = 250f;
        public float restAngle = 0f;
        public float swingFromAngle = 75f;
        public float swingDelay = 0f;
        public float swingDuration = 0.75f;
        public Vector2 spriteSize = new Vector2(120f, 120f);
    }

    [Header("References")]
    [SerializeField] CanvasGroup panelGroup;

    [Header("Decorative Props (sun, moon, stars)")]
    [SerializeField] PropConfig[] decorativeProps;

    [Header("Boss Prop")]
    [SerializeField] PropConfig bossProp;

    [Header("Name Sign")]
    [SerializeField] Sprite signSprite;
    [SerializeField] PropConfig signProp;
    [SerializeField] Vector2 signSize = new Vector2(450f, 160f);
    [SerializeField] TMP_FontAsset signFont;
    [SerializeField] float signFontSize = 48f;
    [SerializeField] Color signTextColor = Color.black;

    [Header("Rope")]
    [SerializeField] Color ropeColor = new Color(0.35f, 0.2f, 0.05f, 1f);
    [SerializeField] float ropeWidth = 6f;
    [SerializeField] int ropeSegments = 5;

    [Header("Shadow")]
    [SerializeField] Vector2 shadowOffset = new Vector2(12f, -12f);
    [SerializeField] Color shadowColor = new Color(0f, 0f, 0f, 0.55f);

    [Header("Backdrop")]
    [SerializeField] Color backdropColor = new Color(0.05f, 0.05f, 0.15f, 0.88f);

    [Header("Pendulum Physics")]
    [SerializeField] float swingOvershoot = 0.25f;
    [SerializeField] float overshootDecay = 0.45f;

    [Header("Idle Sway")]
    [SerializeField] float swayAmplitude = 4f;
    [SerializeField] float swayDuration = 1.8f;

    [Header("Timing")]
    [SerializeField] float exitDuration = 0.4f;
    [SerializeField] float musicFadeInDuration = 0.3f;

    Image backdrop;
    List<RectTransform> pivots = new List<RectTransform>();
    List<float> restAngles = new List<float>();
    List<List<RectTransform>> ropeSegmentGroups = new List<List<RectTransform>>();
    Sequence activeSequence;
    Action pendingOnComplete;
    bool waitingForInput;

    void Awake()
    {
        Instance = this;
        CreateBackdrop();
        panelGroup.gameObject.SetActive(false);
    }

    void CreateBackdrop()
    {
        var go = new GameObject("Backdrop");
        go.transform.SetParent(panelGroup.transform, false);
        backdrop = go.AddComponent<Image>();
        backdrop.color = new Color(backdropColor.r, backdropColor.g, backdropColor.b, 0f);
        backdrop.raycastTarget = true;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.SetAsFirstSibling();
    }

    public void Play(string bossName, Sprite bossSprite, Action onComplete)
    {
        pendingOnComplete = onComplete;
        waitingForInput = false;
        ClearProps();

        backdrop.color = new Color(backdropColor.r, backdropColor.g, backdropColor.b, 0f);

        foreach (var cfg in decorativeProps)
        {
            pivots.Add(SpawnProp(cfg, cfg.sprite));
            restAngles.Add(cfg.restAngle);
        }

        pivots.Add(SpawnProp(bossProp, bossSprite));
        restAngles.Add(bossProp.restAngle);

        pivots.Add(SpawnSign(signProp, bossName));
        restAngles.Add(signProp.restAngle);

        panelGroup.alpha = 1f;
        panelGroup.gameObject.SetActive(true);
        Time.timeScale = 0f;

        BeatConductor.Instance?.FadeParameter("Pause Menu", 1f, musicFadeInDuration);

        activeSequence = BuildSequence();
        activeSequence.OnComplete(() =>
        {
            activeSequence = null;
            waitingForInput = true;
            StartIdleSway();
        });
    }

    void ClearProps()
    {
        foreach (var p in pivots)
            if (p != null) Destroy(p.gameObject);
        pivots.Clear();
        restAngles.Clear();
        ropeSegmentGroups.Clear();
    }

    RectTransform SpawnProp(PropConfig cfg, Sprite sprite)
    {
        var pivot = MakePivot(cfg);
        float segLen = cfg.ropeLength / Mathf.Max(1, ropeSegments);
        var (end, segs) = MakeRopeChain(pivot, cfg.ropeLength);
        ropeSegmentGroups.Add(segs);
        var propImg = MakePropImage(end, sprite, cfg.spriteSize, segLen);
        var shadow = propImg.gameObject.AddComponent<Shadow>();
        shadow.effectColor = shadowColor;
        shadow.effectDistance = shadowOffset;
        return pivot;
    }

    RectTransform SpawnSign(PropConfig cfg, string bossName)
    {
        var pivot = MakePivot(cfg);
        float segLen = cfg.ropeLength / Mathf.Max(1, ropeSegments);
        var (end, segs) = MakeRopeChain(pivot, cfg.ropeLength);
        ropeSegmentGroups.Add(segs);

        var signGO = new GameObject("Sign");
        signGO.transform.SetParent(end, false);
        var signImg = signGO.AddComponent<Image>();
        signImg.sprite = signSprite;
        signImg.type = Image.Type.Simple;
        signImg.raycastTarget = false;
        var signRt = signGO.GetComponent<RectTransform>();
        signRt.pivot = new Vector2(0.5f, 1f);
        signRt.anchorMin = signRt.anchorMax = new Vector2(0.5f, 1f);
        signRt.sizeDelta = signSize;
        signRt.anchoredPosition = new Vector2(0f, -segLen);
        var signShadow = signGO.AddComponent<Shadow>();
        signShadow.effectColor = shadowColor;
        signShadow.effectDistance = shadowOffset;

        var textGO = new GameObject("SignText");
        textGO.transform.SetParent(signGO.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = bossName.ToUpper();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = signFontSize;
        tmp.color = signTextColor;
        tmp.raycastTarget = false;
        if (signFont != null) tmp.font = signFont;
        var textRt = tmp.rectTransform;
        textRt.anchorMin = new Vector2(0.1f, 0.1f);
        textRt.anchorMax = new Vector2(0.9f, 0.9f);
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;

        return pivot;
    }

    RectTransform MakePivot(PropConfig cfg)
    {
        var go = new GameObject("Pivot");
        go.transform.SetParent(panelGroup.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = cfg.pivotPos;
        rt.localRotation = Quaternion.Euler(0f, 0f, cfg.swingFromAngle);
        return rt;
    }

    (Transform end, List<RectTransform> segs) MakeRopeChain(Transform parent, float length)
    {
        var segs = new List<RectTransform>();
        int n = Mathf.Max(1, ropeSegments);
        float segLen = length / n;
        Transform current = parent;

        for (int i = 0; i < n; i++)
        {
            var go = new GameObject($"RopeSeg{i}");
            go.transform.SetParent(current, false);
            var img = go.AddComponent<Image>();
            img.sprite = null;
            img.color = ropeColor;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(ropeWidth, segLen);
            rt.anchoredPosition = i == 0 ? Vector2.zero : new Vector2(0f, -segLen);
            segs.Add(rt);
            current = rt;
        }

        return (current, segs);
    }

    Image MakePropImage(Transform parent, Sprite sprite, Vector2 size, float offset)
    {
        var go = new GameObject("Prop");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.sizeDelta = size;
        rt.anchoredPosition = new Vector2(0f, -offset);
        return img;
    }

    Sequence BuildSequence()
    {
        var seq = DOTween.Sequence().SetUpdate(true);

        seq.Append(backdrop.DOFade(backdropColor.a, 0.25f).SetUpdate(true));

        for (int i = 0; i < decorativeProps.Length; i++)
        {
            var cfg = decorativeProps[i];
            seq.Insert(0.2f + cfg.swingDelay,
                BuildPendulumTween(pivots[i], cfg.swingFromAngle, cfg.restAngle, cfg.swingDuration));
        }

        int bossIdx = decorativeProps.Length;
        seq.Insert(0.2f + bossProp.swingDelay,
            BuildPendulumTween(pivots[bossIdx], bossProp.swingFromAngle, bossProp.restAngle, bossProp.swingDuration));

        int signIdx = bossIdx + 1;
        seq.Insert(0.2f + signProp.swingDelay,
            BuildPendulumTween(pivots[signIdx], signProp.swingFromAngle, signProp.restAngle, signProp.swingDuration));

        return seq;
    }

    Sequence BuildPendulumTween(RectTransform pivot, float startAngle, float restAngle, float duration)
    {
        float travel = startAngle - restAngle;
        float o1 = travel * swingOvershoot;
        float o2 = o1 * overshootDecay;
        float o3 = o2 * overshootDecay;

        float t1 = duration * 0.52f;
        float t2 = duration * 0.24f;
        float t3 = duration * 0.14f;
        float t4 = duration * 0.10f;

        return DOTween.Sequence().SetUpdate(true)
            .Append(pivot.DOLocalRotate(Vector3.forward * (restAngle - o1), t1, RotateMode.Fast)
                .SetEase(Ease.InOutSine).SetUpdate(true))
            .Append(pivot.DOLocalRotate(Vector3.forward * (restAngle + o2), t2, RotateMode.Fast)
                .SetEase(Ease.InOutSine).SetUpdate(true))
            .Append(pivot.DOLocalRotate(Vector3.forward * (restAngle - o3), t3, RotateMode.Fast)
                .SetEase(Ease.InOutSine).SetUpdate(true))
            .Append(pivot.DOLocalRotate(Vector3.forward * restAngle, t4, RotateMode.Fast)
                .SetEase(Ease.OutSine).SetUpdate(true));
    }

    void StartIdleSway()
    {
        float[] rateVariance = { 1.00f, 0.87f, 1.13f, 0.93f, 1.07f, 0.97f };
        for (int i = 0; i < pivots.Count; i++)
        {
            if (pivots[i] == null) continue;
            float rest = restAngles[i];
            float rate = rateVariance[i % rateVariance.Length];
            pivots[i]
                .DOLocalRotate(Vector3.forward * (rest + swayAmplitude), swayDuration * rate)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }
    }

    void Update()
    {
        FlexRopes();

        if (!Input.anyKeyDown) return;

        if (activeSequence != null && activeSequence.IsActive())
        {
            activeSequence.Complete(true);
            return;
        }

        if (waitingForInput)
        {
            waitingForInput = false;
            PlayOutro();
        }
    }

    void FlexRopes()
    {
        for (int p = 0; p < pivots.Count && p < ropeSegmentGroups.Count; p++)
        {
            if (pivots[p] == null) continue;
            var segs = ropeSegmentGroups[p];
            if (segs == null || segs.Count == 0) continue;

            float raw = pivots[p].localEulerAngles.z;
            float A = raw > 180f ? raw - 360f : raw;
            int N = segs.Count;

            segs[0].localEulerAngles = new Vector3(0f, 0f, -A * (N - 1f) / N);
            for (int i = 1; i < N; i++)
                segs[i].localEulerAngles = new Vector3(0f, 0f, A / N);
        }
    }

    void PlayOutro()
    {
        foreach (var p in pivots)
            DOTween.Kill(p);

        BeatConductor.Instance?.FadeParameter("Pause Menu", 0f, exitDuration);

        DOTween.Sequence().SetUpdate(true)
            .Append(panelGroup.DOFade(0f, exitDuration).SetUpdate(true))
            .OnComplete(Finish);
    }

    void Finish()
    {
        panelGroup.gameObject.SetActive(false);
        Time.timeScale = 1f;
        ClearProps();
        pendingOnComplete?.Invoke();
        pendingOnComplete = null;
    }
}
