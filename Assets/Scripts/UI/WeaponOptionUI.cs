using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class WeaponOptionUI : MonoBehaviour, IPointerEnterHandler
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public Image icon;
    public Button button;
    public GameObject selectionBorder;

    [Header("Slide-in")]
    [SerializeField] float slideDistance = 800f;
    [SerializeField] float slideDuration = 0.45f;

    RectTransform rectTransform;
    Vector2 restPosition;

    Weapon currentWeapon;
    int myIndex;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(Weapon weapon, int index)
    {
        currentWeapon = weapon;
        myIndex = index;

        nameText.text = weapon.weaponName;
        descriptionText.text = weapon.description;
        icon.sprite = weapon.icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

    }

    public IEnumerator SlideIn(float delay)
    {
        // Capture rest position now — panel is active and layout is computed
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        restPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = restPosition + Vector2.right * slideDistance;

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        Vector2 start = restPosition + Vector2.right * slideDistance;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            // Ease out back — overshoots target then settles
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float ease = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, restPosition, ease);
            yield return null;
        }

        rectTransform.anchoredPosition = restPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        WeaponSelectUI.Instance.SetSelected(myIndex);
    }

    public void SetHighlighted(bool on)
    {
        if (selectionBorder != null) selectionBorder.SetActive(on);
    }

    public void Select() => WeaponSelectUI.Instance.SelectWeapon(currentWeapon, myIndex);

    void OnClick() => Select();
}
