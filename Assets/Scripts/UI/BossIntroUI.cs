using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class BossIntroUI : MonoBehaviour
{
    public static BossIntroUI Instance;

    [Header("References")]
    [SerializeField] CanvasGroup panelGroup;
    [SerializeField] Image darkOverlay;
    [SerializeField] Image bossPortrait;
    [SerializeField] TextMeshProUGUI bossNameText;
    [SerializeField] TextMeshProUGUI bossSubtitleText;
    [SerializeField] RectTransform nameBarRt;

    [Header("Lines")]
    [SerializeField] int lineCount = 7;
    [SerializeField] float lineMinWidth = 25f;
    [SerializeField] float lineMaxWidth = 300f;
    [SerializeField] float lineHeight = 2500f;
    [SerializeField] float lineMinAngle = -60f;
    [SerializeField] float lineMaxAngle = 60f;
    [SerializeField] Color lineColor = new Color(0.35f, 0f, 0f, 1f);
    [SerializeField] bool useLineColor2 = false;
    [SerializeField] Color lineColor2 = Color.white;
    [SerializeField] bool useLineColor3 = false;
    [SerializeField] Color lineColor3 = Color.white;
    [SerializeField] float lineMaxSkew = 80f;
    [SerializeField] float lineSkewChance = 0.6f;
    [SerializeField] float canvasHalfWidth = 960f;

    [Header("Name Bar")]
    [SerializeField] float barTextOffsetX = 135f;
    [SerializeField] float barTextOffsetY = -15f;

    [Header("Name Text")]
    [SerializeField] float textWidth = 1400f;
    [SerializeField] float textLeftScale = 1f;
    [SerializeField] float textRightScale = 0.5f;

    [Header("Portrait")]
    [SerializeField] float portraitSpriteScale = 6f;
    [SerializeField] float portraitTargetX = -280f;

    [Header("Labels")]
    [SerializeField] string subtitleLabel = "MINIBOSS";

    [Header("Timing")]
    [SerializeField] float exitDuration = 0.35f;

    [Header("Music")]
    [SerializeField] float musicFadeInDuration = 0.3f;

    const float OffscreenOffset = 2500f;

    RectTransform[] lines;
    Vector2[] lineTargetPositions;
    float barTargetY;
    float barTiltAngle;
    Sequence activeSequence;
    Action pendingOnComplete;
    bool waitingForInput;

    void Awake()
    {
        Instance = this;

        if (panelGroup == null)
            panelGroup = GetComponent<CanvasGroup>();

        if (panelGroup == null)
        {
            Debug.LogError("BossIntroUI: no CanvasGroup found on this GameObject.");
            return;
        }

        if (nameBarRt == null)
        {
            Debug.LogError("BossIntroUI: nameBarRt is not assigned in the Inspector.");
            return;
        }

        // Read the bar's resting position and tilt from the scene object
        barTargetY = nameBarRt.anchoredPosition.y;

        var skewedBar = nameBarRt.GetComponent<SkewedBar>();
        if (skewedBar != null)
        {
            float yDiff = skewedBar.rightYOffset - skewedBar.leftYOffset;
            barTiltAngle = Mathf.Atan2(yDiff, nameBarRt.sizeDelta.x) * Mathf.Rad2Deg;
        }

        BuildLines();
        SetupTextTaper();
        ArrangeSiblings();
        panelGroup.gameObject.SetActive(false);
    }

    void BuildLines()
    {
        var palette = new System.Collections.Generic.List<Color> { lineColor };
        if (useLineColor2) palette.Add(lineColor2);
        if (useLineColor3) palette.Add(lineColor3);

        lines = new RectTransform[lineCount];
        lineTargetPositions = new Vector2[lineCount];

        float segmentWidth = (canvasHalfWidth * 2f) / lineCount;

        for (int i = 0; i < lineCount; i++)
        {
            var go = new GameObject($"Line_{i}");
            go.transform.SetParent(panelGroup.transform, false);

            var bar = go.AddComponent<SkewedBar>();
            bar.color = palette[UnityEngine.Random.Range(0, palette.Count)];
            bar.raycastTarget = false;

            if (UnityEngine.Random.value < lineSkewChance)
            {
                bar.topLeftSkew  = UnityEngine.Random.Range(-lineMaxSkew, lineMaxSkew);
                bar.topRightSkew = UnityEngine.Random.Range(-lineMaxSkew, lineMaxSkew);
            }

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(
                UnityEngine.Random.Range(lineMinWidth, lineMaxWidth),
                lineHeight);

            float segCenter = -canvasHalfWidth + segmentWidth * (i + 0.5f);
            float xJitter = UnityEngine.Random.Range(-segmentWidth * 0.4f, segmentWidth * 0.4f);
            rt.anchoredPosition = new Vector2(segCenter + xJitter, 0f);
            rt.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(lineMinAngle, lineMaxAngle));

            lines[i] = rt;
            lineTargetPositions[i] = rt.anchoredPosition;
        }
    }

    void SetupTextTaper()
    {
        if (bossNameText == null) return;
        var taper = bossNameText.GetComponent<TaperedText>()
                    ?? bossNameText.gameObject.AddComponent<TaperedText>();
        taper.leftScale  = textLeftScale;
        taper.rightScale = textRightScale;
    }

    void ArrangeSiblings()
    {
        // In Canvas, last child renders on top.
        // Order: DarkOverlay → vertical Lines → Portrait → NameBar → BossName
        darkOverlay.transform.SetSiblingIndex(0);
        for (int i = 0; i < lines.Length; i++)
            lines[i].SetSiblingIndex(i + 1);
        bossPortrait.transform.SetSiblingIndex(lines.Length + 1);
        nameBarRt.SetSiblingIndex(lines.Length + 2);
        bossNameText.transform.SetSiblingIndex(lines.Length + 3);
    }

    public void Play(string bossName, Sprite bossSprite, Action onComplete)
    {
        if (panelGroup == null || darkOverlay == null || bossPortrait == null
            || bossNameText == null || nameBarRt == null)
        {
            Debug.LogWarning("BossIntroUI: one or more Inspector references are missing — skipping cutscene.");
            onComplete?.Invoke();
            return;
        }

        pendingOnComplete = onComplete;
        waitingForInput = false;

        SetupContent(bossName, bossSprite);
        ResetInitialStates();

        panelGroup.alpha = 1f;
        panelGroup.gameObject.SetActive(true);
        Time.timeScale = 0f;

        BeatConductor.Instance?.FadeParameter("Pause Menu", 1f, musicFadeInDuration);

        activeSequence = BuildIntroSequence();
        activeSequence.OnComplete(() => { activeSequence = null; waitingForInput = true; });
    }

    void SetupContent(string bossName, Sprite bossSprite)
    {
        bossNameText.text = bossName.ToUpper();

        if (bossSubtitleText != null)
            bossSubtitleText.text = subtitleLabel;

        if (bossSprite != null)
        {
            bossPortrait.sprite = bossSprite;
            bossPortrait.SetNativeSize();
            bossPortrait.rectTransform.sizeDelta *= portraitSpriteScale;
        }
    }

    void ResetInitialStates()
    {
        darkOverlay.color = new Color(0f, 0f, 0f, 0f);

        bossPortrait.rectTransform.anchoredPosition = new Vector2(portraitTargetX - OffscreenOffset, 0f);
        bossPortrait.color = Color.white;

        // Bar starts off-screen below and slides up to its scene position
        nameBarRt.anchoredPosition = new Vector2(nameBarRt.anchoredPosition.x, barTargetY - OffscreenOffset);

        // Text sits on bar, sized and rotated to match
        float barHeight = nameBarRt.sizeDelta.y;
        bossNameText.enableWordWrapping = false;
        bossNameText.rectTransform.sizeDelta = new Vector2(textWidth, barHeight);
        bossNameText.rectTransform.anchoredPosition = new Vector2(barTextOffsetX, barTargetY + barTextOffsetY);
        bossNameText.rectTransform.localRotation = Quaternion.Euler(0f, 0f, barTiltAngle);
        bossNameText.alpha = 0f;
        bossNameText.rectTransform.localScale = Vector3.one * 1.5f;

        if (bossSubtitleText != null)
            bossSubtitleText.alpha = 0f;

        for (int i = 0; i < lines.Length; i++)
        {
            float dir = (i % 2 == 0) ? -1f : 1f;
            lines[i].anchoredPosition = new Vector2(
                lineTargetPositions[i].x + dir * OffscreenOffset,
                lineTargetPositions[i].y);
        }
    }

    Sequence BuildIntroSequence()
    {
        Sequence seq = DOTween.Sequence().SetUpdate(true);

        seq.Append(darkOverlay.DOFade(0.75f, 0.2f).SetUpdate(true));

        for (int i = 0; i < lines.Length; i++)
        {
            seq.Insert(0.15f + i * 0.045f,
                lines[i].DOAnchorPos(lineTargetPositions[i], 0.12f)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true));
        }

        seq.Insert(0.3f,
            bossPortrait.rectTransform.DOAnchorPosX(portraitTargetX, 0.35f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true));

        seq.Insert(0.45f,
            nameBarRt.DOAnchorPosY(barTargetY, 0.18f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true));

        seq.Insert(0.65f, bossNameText.DOFade(1f, 0.1f).SetUpdate(true));
        seq.Insert(0.65f,
            bossNameText.rectTransform.DOScale(1f, 0.25f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true));

        if (bossSubtitleText != null && !string.IsNullOrEmpty(subtitleLabel))
            seq.Insert(0.8f, bossSubtitleText.DOFade(1f, 0.2f).SetUpdate(true));

        return seq;
    }

    void Update()
    {
        if (!Input.anyKeyDown) return;

        if (activeSequence != null && activeSequence.IsActive() && !activeSequence.IsComplete())
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

    void PlayOutro()
    {
        BeatConductor.Instance?.FadeParameter("Pause Menu", 0f, exitDuration);

        DOTween.Sequence()
            .SetUpdate(true)
            .Append(panelGroup.DOFade(0f, exitDuration).SetUpdate(true))
            .OnComplete(Finish);
    }

    void Finish()
    {
        panelGroup.gameObject.SetActive(false);
        Time.timeScale = 1f;
        activeSequence = null;
        pendingOnComplete?.Invoke();
        pendingOnComplete = null;
    }
}
