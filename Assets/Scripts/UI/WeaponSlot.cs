using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public struct GroupContainerSprite
{
    public string groupId;
    public Sprite unselectedSprite;
    public Sprite selectedSprite;
}

public class WeaponSlot : MonoBehaviour
{
    [Header("References")]
    public Image icon;
    public Image outline;
    public Image containerImage;
    public RectTransform contentRect;  // child that wraps all visuals — this bobs

    [Header("Container Sprites")]
    public Sprite defaultContainerSprite;
    public GroupContainerSprite[] containerSprites;

    [Header("Visual Settings")]
    public float selectedScale   = 1.2f;
    public float normalScale     = 1f;
    public float selectedAlpha   = 1f;
    public float unselectedAlpha = 0.5f;
    public Color emptyColor      = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Beat Punch")]
    [SerializeField] float punchDistance = 8f;
    [SerializeField] float punchDuration = 0.35f;

    bool hasWeapon;
    bool isSelected;
    EnemyType currentWeaponType;
    string currentGroupId;
    Vector2 restPosition;
    Coroutine punchCoroutine;

    static readonly Color BlueColor   = new Color(0.20f, 0.53f, 1.00f);
    static readonly Color RedColor    = new Color(1.00f, 0.22f, 0.22f);
    static readonly Color GreenColor  = new Color(0.18f, 0.85f, 0.35f);
    static readonly Color YellowColor = new Color(1.00f, 0.85f, 0.10f);

    void Start()
    {
        if (containerImage != null)
            containerImage.sprite = defaultContainerSprite;
    }

    void OnDisable()
    {
        if (isSelected) BeatConductor.OnBeat -= OnBeat;
        isSelected = false;
        StopPunch();
    }

    public void SetWeapon(Sprite weaponIcon, EnemyType weaponType = EnemyType.None, string groupId = "")
    {
        hasWeapon         = weaponIcon != null;
        currentWeaponType = weaponType;
        currentGroupId    = groupId;
        icon.sprite       = weaponIcon;
        icon.color        = hasWeapon ? new Color(1f, 1f, 1f, unselectedAlpha) : emptyColor;
        SetOutlineColor(weaponType);
        SetContainerSprite();
    }

    public void SetSelected(bool selected)
    {
        transform.localScale = Vector3.one * (selected ? selectedScale : normalScale);
        if (hasWeapon)
            SetAlpha(selected ? selectedAlpha : unselectedAlpha);

        if (selected == isSelected) return;
        isSelected = selected;

        SetContainerSprite();

        if (selected)
            BeatConductor.OnBeat += OnBeat;
        else
        {
            BeatConductor.OnBeat -= OnBeat;
            StopPunch();
        }
    }

    void SetContainerSprite()
    {
        if (containerImage == null) return;

        if (!hasWeapon || string.IsNullOrEmpty(currentGroupId))
        {
            containerImage.sprite = defaultContainerSprite;
            return;
        }

        foreach (var entry in containerSprites)
        {
            if (entry.groupId != currentGroupId) continue;
            containerImage.sprite = isSelected ? entry.selectedSprite : entry.unselectedSprite;
            return;
        }

        containerImage.sprite = defaultContainerSprite;
    }

    void OnBeat()
    {
        if (punchCoroutine == null)
            restPosition = contentRect.anchoredPosition;
        else
            StopCoroutine(punchCoroutine);

        punchCoroutine = StartCoroutine(PunchCoroutine());
    }

    IEnumerator PunchCoroutine()
    {
        const float peakFraction = 0.25f;
        float elapsed = 0f;

        while (elapsed < punchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / punchDuration);

            float y;
            if (t < peakFraction)
            {
                float t1 = t / peakFraction;
                y = punchDistance * EaseOutQuad(t1);
            }
            else
            {
                float t2 = (t - peakFraction) / (1f - peakFraction);
                y = punchDistance * (1f - EaseOutBack(t2));
            }

            contentRect.anchoredPosition = restPosition + Vector2.up * y;
            yield return null;
        }

        contentRect.anchoredPosition = restPosition;
        punchCoroutine = null;
    }

    void StopPunch()
    {
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
            punchCoroutine = null;
        }
        if (contentRect != null)
            contentRect.anchoredPosition = restPosition;
    }

    static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    void SetAlpha(float alpha)
    {
        Color c = icon.color;
        c.a = alpha;
        icon.color = c;
    }

    void SetOutlineColor(EnemyType weaponType)
    {
        if (outline == null) return;

        outline.color = weaponType switch
        {
            EnemyType.Blue   => BlueColor,
            EnemyType.Red    => RedColor,
            EnemyType.Green  => GreenColor,
            EnemyType.Yellow => YellowColor,
            _                => new Color(0.5f, 0.5f, 0.5f, 1f)
        };
    }
}
