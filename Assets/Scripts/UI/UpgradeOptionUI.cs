using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class UpgradeOptionUI : MonoBehaviour, IPointerEnterHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image icon;
    public Button button;
    public GameObject selectionBorder;

    [Header("Slide-in")]
    [SerializeField] float slideDistance = 800f;
    [SerializeField] float slideDuration = 0.45f;

    RectTransform rectTransform;
    Vector2 restPosition;

    UpgradeData currentUpgrade;
    int myIndex;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(UpgradeData upgrade, int index)
    {
        currentUpgrade = upgrade;
        myIndex = index;

        bool valid = upgrade != null;
        gameObject.SetActive(valid);
        if (!valid) return;

        nameText.text        = upgrade.upgradeName;
        descriptionText.text = upgrade.description;
        icon.sprite          = upgrade.icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        // Snap off-screen so it's ready to slide in
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        restPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = restPosition + Vector2.down * slideDistance;
    }

    public IEnumerator SlideIn(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        Vector2 start = restPosition + Vector2.down * slideDistance;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;
            // Ease out cubic — fast start, settles smoothly
            t = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, restPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = restPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpgradeUI.Instance.SetSelected(myIndex);
    }

    public void SetHighlighted(bool on)
    {
        if (selectionBorder != null) selectionBorder.SetActive(on);
    }

    public void Select() => UpgradeUI.Instance.SelectUpgrade(currentUpgrade);

    void OnClick() => Select();
}