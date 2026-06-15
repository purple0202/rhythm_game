using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class PassiveOptionUI : MonoBehaviour, IPointerEnterHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image icon;
    public Button button;
    public GameObject selectionBorder;

    [Header("Slide-in")]
    [SerializeField] float slideDistance = 800f;
    [SerializeField] float slideDuration = 0.45f;

    [Header("Slide-out")]
    [SerializeField] float slideOutDuration = 0.3f;
    public float SlideOutDuration => slideOutDuration;

    RectTransform rectTransform;
    Vector2 restPosition;
    bool restPositionCached;

    PassiveData currentPassive;
    int myIndex;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(PassiveData passive, int index)
    {
        currentPassive = passive;
        myIndex = index;
        nameText.text = passive.passiveName;
        descriptionText.text = passive.description;
        icon.sprite = passive.icon;
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    // Called after panel is active and layout has settled
    public IEnumerator SlideIn(float delay)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        // First show: capture the layout position. Subsequent shows: restore it.
        if (!restPositionCached)
        {
            restPosition = rectTransform.anchoredPosition;
            restPositionCached = true;
        }
        else
        {
            rectTransform.anchoredPosition = restPosition;
        }
        Vector2 start = restPosition + Vector2.down * slideDistance;

        // Snap off-screen before delay so there's no flash
        rectTransform.anchoredPosition = start;

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, restPosition, EaseOutBack(t));
            yield return null;
        }

        rectTransform.anchoredPosition = restPosition;
    }

    public IEnumerator SlideOut(float delay)
    {
        Vector2 target = restPosition + Vector2.down * slideDistance;

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideOutDuration);
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(restPosition, target, EaseInBack(t));
            yield return null;
        }

        rectTransform.anchoredPosition = target;
    }

    // Overshoots slightly then settles — gives the inertia feel
    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // Mirror of EaseOutBack — pulls back briefly before launching off-screen
    static float EaseInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PassiveSelectionUI.Instance.SetSelected(myIndex);
    }

    public void SetHighlighted(bool on)
    {
        if (selectionBorder != null) selectionBorder.SetActive(on);
    }

    public void Select() => PassiveSelectionUI.Instance.SelectPassive(currentPassive);

    void OnClick() => Select();
}
