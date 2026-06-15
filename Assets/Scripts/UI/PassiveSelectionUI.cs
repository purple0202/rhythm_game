using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PassiveSelectionUI : MonoBehaviour
{
    public static PassiveSelectionUI Instance;

    public GameObject panel;
    public PassiveOptionUI[] optionSlots;

    [Header("Backdrop")]
    [SerializeField] Image backdrop;
    [SerializeField] float backdropTargetAlpha = 0.7f;
    [SerializeField] float backdropFadeDuration = 0.3f;

    [Header("Slide-in")]
    [SerializeField] float staggerDelay = 0.08f;

    [Header("Grace Periods")]
    public float preUIGraceSeconds = 1f;
    public int postUIGraceBeats = 1;

    int selectedIndex;
    int visibleSlotCount;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        if (backdrop != null)
        {
            Color c = backdrop.color;
            c.a = 0f;
            backdrop.color = c;
        }
    }

    public void Show(PassiveData[] options)
    {
        visibleSlotCount = options.Length;

        for (int i = 0; i < optionSlots.Length; i++)
        {
            bool visible = i < options.Length;
            optionSlots[i].gameObject.SetActive(visible);
            if (visible) optionSlots[i].Setup(options[i], i);
        }

        Time.timeScale = 0f;
        StartCoroutine(PreGraceCoroutine());
    }

    IEnumerator PreGraceCoroutine()
    {
        BeatConductor.Instance.FadeParameter("Pause Menu", 1f, preUIGraceSeconds);
        yield return new WaitForSecondsRealtime(preUIGraceSeconds);

        selectedIndex = 0;
        panel.SetActive(true);

        // Wait one frame so Unity's layout system settles before we read positions
        yield return null;

        FocusSelected();

        if (backdrop != null)
            StartCoroutine(FadeBackdrop(0f, backdropTargetAlpha, backdropFadeDuration));

        for (int i = 0; i < optionSlots.Length; i++)
        {
            if (optionSlots[i].gameObject.activeSelf)
                StartCoroutine(optionSlots[i].SlideIn(i * staggerDelay));
        }
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            Navigate(-1);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            Navigate(1);

        if (Input.GetKeyDown(KeyCode.E))
            optionSlots[selectedIndex].Select();
    }

    public void SetSelected(int index)
    {
        optionSlots[selectedIndex].SetHighlighted(false);
        selectedIndex = index;
        FocusSelected();
    }

    void Navigate(int dir)
    {
        selectedIndex = (selectedIndex + dir + visibleSlotCount) % visibleSlotCount;
        FocusSelected();
    }

    void FocusSelected()
    {
        foreach (var slot in optionSlots)
            slot.SetHighlighted(false);
        EventSystem.current.SetSelectedGameObject(optionSlots[selectedIndex].button.gameObject);
        optionSlots[selectedIndex].SetHighlighted(true);
    }

    public void SelectPassive(PassiveData data)
    {
        // Disable all buttons immediately to prevent double-selection during slide-out
        foreach (var slot in optionSlots)
            if (slot.gameObject.activeSelf) slot.button.interactable = false;

        StartCoroutine(SelectPassiveCoroutine(data));
    }

    IEnumerator SelectPassiveCoroutine(PassiveData data)
    {
        float maxWait = 0f;
        int staggerIndex = 0;
        for (int i = 0; i < optionSlots.Length; i++)
        {
            if (!optionSlots[i].gameObject.activeSelf) continue;
            if (i == selectedIndex) continue;

            float delay = staggerIndex * staggerDelay;
            StartCoroutine(optionSlots[i].SlideOut(delay));
            maxWait = Mathf.Max(maxWait, delay + optionSlots[i].SlideOutDuration);
            staggerIndex++;
        }

        yield return new WaitForSecondsRealtime(maxWait);

        PassiveManager.Instance.Equip(data);
        panel.SetActive(false);
        if (backdrop != null)
            StartCoroutine(FadeBackdrop(backdropTargetAlpha, 0f, backdropFadeDuration));
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

    IEnumerator FadeBackdrop(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = backdrop.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            backdrop.color = c;
            yield return null;
        }

        c.a = to;
        backdrop.color = c;
    }
}
