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

    [Header("Lines")]
    [SerializeField] int lineCount = 7;
    [SerializeField] float lineMinWidth = 25f;
    [SerializeField] float lineMaxWidth = 300f;
    [SerializeField] float lineHeight = 2500f;
    [SerializeField] float lineMinAngle = -60f;
    [SerializeField] float lineMaxAngle = 60f;
    [SerializeField] Color lineColor = new Color(0.35f, 0f, 0f, 1f);
    [SerializeField] float canvasHalfWidth = 960f;

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

        BuildLines();
        panelGroup.gameObject.SetActive(false);
    }

    void BuildLines()
    {
        lines = new RectTransform[lineCount];
        lineTargetPositions = new Vector2[lineCount];

        float segmentWidth = (canvasHalfWidth * 2f) / lineCount;

        for (int i = 0; i < lineCount; i++)
        {
            var go = new GameObject($"Line_{i}");
            go.transform.SetParent(panelGroup.transform, false);

            var img = go.AddComponent<Image>();
            img.color = lineColor;
            img.raycastTarget = false;

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

    public void Play(string bossName, Sprite bossSprite, Action onComplete)
    {
        if (panelGroup == null || darkOverlay == null || bossPortrait == null || bossNameText == null)
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

        seq.Insert(0.55f, bossNameText.DOFade(1f, 0.1f).SetUpdate(true));
        seq.Insert(0.55f,
            bossNameText.rectTransform.DOScale(1f, 0.25f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true));

        if (bossSubtitleText != null && !string.IsNullOrEmpty(subtitleLabel))
            seq.Insert(0.7f, bossSubtitleText.DOFade(1f, 0.2f).SetUpdate(true));

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
