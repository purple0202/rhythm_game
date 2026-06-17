using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BossCutsceneUI : MonoBehaviour
{
    public static BossCutsceneUI Instance;

    [Header("References")]
    [SerializeField] GameObject panel;
    [SerializeField] Image darkBackdrop;
    [SerializeField] RectTransform leftCurtain;
    [SerializeField] RectTransform rightCurtain;
    [SerializeField] RectTransform topCurtain;
    [SerializeField] RectTransform[] stars;
    [SerializeField] Image bossImage;
    [SerializeField] TextMeshProUGUI bossNameText;

    [Header("Backdrop")]
    [SerializeField] float backdropTargetAlpha = 0.75f;
    [SerializeField] float backdropFadeInDuration = 0.2f;
    [SerializeField] float backdropFadeOutDuration = 0.25f;

    [Header("Curtains")]
    [SerializeField] float curtainInDuration = 0.4f;
    [SerializeField] float curtainOutDuration = 0.25f;
    [SerializeField] float postCurtainDelay = 0.1f;

    [Header("Stars")]
    [SerializeField] Vector2 starPositionRangeX = new Vector2(-700f, 700f);
    [SerializeField] Vector2 starPositionRangeY = new Vector2(-350f, 350f);
    [SerializeField] Vector2 starScaleRange = new Vector2(0.5f, 1.5f);
    [SerializeField] float starDropHeight = 300f;
    [SerializeField] float starPopDuration = 0.2f;
    [SerializeField] float starStagger = 0.08f;
    [SerializeField] float postStarsDelay = 0.15f;

    [Header("Boss Image")]
    [SerializeField] float bossImageDuration = 0.5f;
    [SerializeField] float bossImageStartOffsetY = 1200f;
    [SerializeField] float postBossImageDelay = 0.1f;

    [Header("Boss Name Slam")]
    [SerializeField] float nameSlamDuration = 0.18f;
    [SerializeField] float nameSlamPunchStrength = 0.5f;

    Vector2 leftCurtainRest, rightCurtainRest, topCurtainRest;
    Vector2 bossImageRest;
    Vector2 bossNameRest;
    Vector3 bossNameRestScale;

    Sequence activeSequence;
    Action pendingOnComplete;
    bool waitingForInput;

    void Awake()
    {
        Instance = this;

        if (leftCurtain != null) leftCurtainRest = leftCurtain.anchoredPosition;
        if (rightCurtain != null) rightCurtainRest = rightCurtain.anchoredPosition;
        if (topCurtain != null) topCurtainRest = topCurtain.anchoredPosition;
        if (bossImage != null) bossImageRest = bossImage.rectTransform.anchoredPosition;
        if (bossNameText != null)
        {
            bossNameRest = bossNameText.rectTransform.anchoredPosition;
            bossNameRestScale = bossNameText.rectTransform.localScale;
        }

        if (panel != null)
            panel.SetActive(false);
    }

    public void Play(string bossName, Sprite bossSprite, Action onComplete)
    {
        if (panel == null)
        {
            Debug.LogWarning("BossCutsceneUI: panel is not assigned in the Inspector — skipping cutscene.");
            onComplete?.Invoke();
            return;
        }

        pendingOnComplete = onComplete;
        waitingForInput = false;

        if (bossNameText != null)
            bossNameText.text = bossName.ToUpper();

        if (bossImage != null && bossSprite != null)
            bossImage.sprite = bossSprite;

        ResetInitialStates();

        panel.SetActive(true);
        Time.timeScale = 0f;

        activeSequence = BuildIntroSequence();
        activeSequence.OnComplete(() => { activeSequence = null; waitingForInput = true; });
    }

    void ResetInitialStates()
    {
        if (darkBackdrop != null)
        {
            Color c = darkBackdrop.color;
            c.a = 0f;
            darkBackdrop.color = c;
        }

        if (leftCurtain != null)
            leftCurtain.anchoredPosition = leftCurtainRest + Vector2.left * leftCurtain.rect.width;
        if (rightCurtain != null)
            rightCurtain.anchoredPosition = rightCurtainRest + Vector2.right * rightCurtain.rect.width;
        if (topCurtain != null)
            topCurtain.anchoredPosition = topCurtainRest + Vector2.up * topCurtain.rect.height;

        if (stars != null)
            foreach (var star in stars)
                if (star != null) star.gameObject.SetActive(false);

        if (bossImage != null)
            bossImage.rectTransform.anchoredPosition = bossImageRest + Vector2.down * bossImageStartOffsetY;

        if (bossNameText != null)
        {
            bossNameText.rectTransform.anchoredPosition = bossNameRest;
            bossNameText.rectTransform.localScale = bossNameRestScale;
            bossNameText.alpha = 0f;
        }
    }

    Sequence BuildIntroSequence()
    {
        Sequence seq = DOTween.Sequence().SetUpdate(true);

        if (darkBackdrop != null)
            seq.Insert(0f, darkBackdrop.DOFade(backdropTargetAlpha, backdropFadeInDuration).SetUpdate(true));

        if (leftCurtain != null)
            seq.Insert(0f, leftCurtain.DOAnchorPos(leftCurtainRest, curtainInDuration).SetEase(Ease.OutCubic).SetUpdate(true));
        if (rightCurtain != null)
            seq.Insert(0f, rightCurtain.DOAnchorPos(rightCurtainRest, curtainInDuration).SetEase(Ease.OutCubic).SetUpdate(true));
        if (topCurtain != null)
            seq.Insert(0f, topCurtain.DOAnchorPos(topCurtainRest, curtainInDuration).SetEase(Ease.OutCubic).SetUpdate(true));

        float starsStart = curtainInDuration + postCurtainDelay;
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] == null) continue;
                RectTransform star = stars[i];
                seq.InsertCallback(starsStart + i * starStagger, () => PopStar(star));
            }
        }

        int starCount = stars != null ? stars.Length : 0;
        float bossImageStart = starsStart + starCount * starStagger + postStarsDelay;
        if (bossImage != null)
            seq.Insert(bossImageStart,
                bossImage.rectTransform.DOAnchorPos(bossImageRest, bossImageDuration)
                    .SetEase(Ease.OutCubic).SetUpdate(true));

        float nameStart = bossImageStart + bossImageDuration + postBossImageDelay;
        if (bossNameText != null)
        {
            seq.InsertCallback(nameStart, () => bossNameText.alpha = 1f);
            seq.Insert(nameStart,
                bossNameText.rectTransform.DOPunchScale(bossNameRestScale * nameSlamPunchStrength, nameSlamDuration, 1, 0.5f)
                    .SetUpdate(true));
        }

        return seq;
    }

    void PopStar(RectTransform star)
    {
        Vector2 targetPos = new Vector2(
            UnityEngine.Random.Range(starPositionRangeX.x, starPositionRangeX.y),
            UnityEngine.Random.Range(starPositionRangeY.x, starPositionRangeY.y));

        star.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
        star.localScale = Vector3.one * UnityEngine.Random.Range(starScaleRange.x, starScaleRange.y);
        star.anchoredPosition = targetPos + Vector2.up * starDropHeight;
        star.gameObject.SetActive(true);

        star.DOAnchorPos(targetPos, starPopDuration).SetEase(Ease.OutBounce).SetUpdate(true);
    }

    void Update()
    {
        if (panel == null || !panel.activeSelf) return;
        if (!Input.GetKeyDown(KeyCode.E) && !Input.GetMouseButtonDown(0)) return;

        if (activeSequence != null && activeSequence.IsActive() && !activeSequence.IsComplete())
        {
            activeSequence.Complete(true);
            return;
        }

        if (waitingForInput)
        {
            waitingForInput = false;
            PlayExit();
        }
    }

    void PlayExit()
    {
        Sequence exit = DOTween.Sequence().SetUpdate(true);

        if (darkBackdrop != null)
            exit.Insert(0f, darkBackdrop.DOFade(0f, backdropFadeOutDuration).SetUpdate(true));

        if (leftCurtain != null)
            exit.Insert(0f, leftCurtain.DOAnchorPos(leftCurtainRest + Vector2.left * leftCurtain.rect.width, curtainOutDuration).SetEase(Ease.InCubic).SetUpdate(true));
        if (rightCurtain != null)
            exit.Insert(0f, rightCurtain.DOAnchorPos(rightCurtainRest + Vector2.right * rightCurtain.rect.width, curtainOutDuration).SetEase(Ease.InCubic).SetUpdate(true));
        if (topCurtain != null)
            exit.Insert(0f, topCurtain.DOAnchorPos(topCurtainRest + Vector2.up * topCurtain.rect.height, curtainOutDuration).SetEase(Ease.InCubic).SetUpdate(true));

        exit.OnComplete(Finish);
    }

    void Finish()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        activeSequence = null;
        pendingOnComplete?.Invoke();
        pendingOnComplete = null;
    }
}
