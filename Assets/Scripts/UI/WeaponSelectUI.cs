using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class WeaponSelectUI : MonoBehaviour
{
    public static WeaponSelectUI Instance;

    public GameObject panel;
    public WeaponOptionUI[] optionSlots;

    [Header("Grace Periods")]
    public float preUIGraceSeconds = 1f;
    public int postUIGraceBeats = 1;

    [Header("Focus Panel")]
    public Image focusIcon;
    public TMP_Text focusName;
    public TMP_Text focusDescription;
    public float spinSpeed = 30f;

    [Header("Slide Animation")]
    public RectTransform focusPanel;
    public float slideDuration = 0.4f;
    public float slideOffset = 300f;
    public float staggerDelay = 0.08f;

    WeaponController pendingController;
    string pendingFmodParam;
    int[] pendingFmodValues;
    int selectedIndex;
    Weapon[] currentWeapons;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(Weapon[] weapons, string fmodParam, int[] fmodValues, WeaponController controller)
    {
        currentWeapons = weapons;
        pendingController = controller;
        pendingFmodParam = fmodParam;
        pendingFmodValues = fmodValues;

        for (int i = 0; i < optionSlots.Length; i++)
            optionSlots[i].Setup(weapons[i], i);

        StartCoroutine(PreGraceCoroutine());
    }

    IEnumerator PreGraceCoroutine()
    {
        BeatConductor.Instance.FadeParameter("Pause Menu", 1f, preUIGraceSeconds);
        yield return new WaitForSecondsRealtime(preUIGraceSeconds);
        selectedIndex = 0;
        panel.SetActive(true);
        Canvas.ForceUpdateCanvases();
        FocusSelected();
        if (focusPanel != null) StartCoroutine(SlideIn(focusPanel, Vector2.down * slideOffset));
        for (int i = 0; i < optionSlots.Length; i++)
            StartCoroutine(optionSlots[i].SlideIn(i * staggerDelay));
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            Navigate(-1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            Navigate(1);

        if (Input.GetKeyDown(KeyCode.P))
            optionSlots[selectedIndex].Select();

        if (focusIcon != null)
            focusIcon.transform.Rotate(0f, 0f, -spinSpeed * Time.unscaledDeltaTime);
    }

    public void SetSelected(int index)
    {
        optionSlots[selectedIndex].SetHighlighted(false);
        selectedIndex = index;
        FocusSelected();
    }

    void Navigate(int dir)
    {
        SetSelected((selectedIndex + dir + optionSlots.Length) % optionSlots.Length);
    }

    void FocusSelected()
    {
        foreach (var slot in optionSlots)
            slot.SetHighlighted(false);
        EventSystem.current.SetSelectedGameObject(optionSlots[selectedIndex].button.gameObject);
        optionSlots[selectedIndex].SetHighlighted(true);
        UpdateFocus(currentWeapons[selectedIndex]);
    }

    IEnumerator SlideIn(RectTransform rect, Vector2 fromOffset)
    {
        Vector2 target = rect.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            rect.anchoredPosition = Vector2.LerpUnclamped(target + fromOffset, target, t);
            yield return null;
        }
        rect.anchoredPosition = target;
    }

    void UpdateFocus(Weapon weapon)
    {
        if (focusIcon != null) focusIcon.sprite = weapon.icon;
        if (focusName != null) focusName.text = weapon.weaponName;
        if (focusDescription != null) focusDescription.text = weapon.description;
    }

    // Called by WeaponOptionUI — optionIndex is 0-based, FMOD value is 1-based
    public void SelectWeapon(Weapon weapon, int optionIndex)
    {
        pendingController.EquipGroupWeapon(weapon, pendingFmodParam, pendingFmodValues[optionIndex]);
        panel.SetActive(false);
        StartCoroutine(PostGraceCoroutine());
    }

    IEnumerator PostGraceCoroutine()
    {
        float resumeDuration = BeatConductor.Instance.secondsPerBeat > 0
            ? BeatConductor.Instance.secondsPerBeat * postUIGraceBeats
            : 1f;
        BeatConductor.Instance.FadeParameter("Pause Menu", 0f, resumeDuration);

        int beatsRemaining = postUIGraceBeats;
        System.Action onBeat = () => beatsRemaining--;
        BeatConductor.OnBeat += onBeat;
        yield return new WaitUntil(() => beatsRemaining <= 0);
        BeatConductor.OnBeat -= onBeat;
        Time.timeScale = 1f;
    }
}
