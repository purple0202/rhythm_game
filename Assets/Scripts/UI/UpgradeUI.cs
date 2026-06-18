using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance;

    public GameObject panel;
    public UpgradeOptionUI[] optionSlots;

    [Header("Reroll Button")]
    public GameObject rerollButton;
    public TextMeshProUGUI rerollButtonText;

    [Header("Grace Periods")]
    public float preUIGraceSeconds = 1f;
    public int postUIGraceBeats = 1;

    int selectedIndex;
    List<UpgradeData> currentOptions = new();
    bool pendingWeaponSelect = false;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show()
    {
        UpgradeManager.Instance.ResetRerolls();
        ShowOptions(UpgradeManager.Instance.GetRandomOptions());
    }

    void ShowOptions(List<UpgradeData> options)
    {
        currentOptions = options;
        for (int i = 0; i < optionSlots.Length; i++)
            optionSlots[i].Setup(i < options.Count ? options[i] : null, i);

        StartCoroutine(PreGraceCoroutine());
    }

    public void Reroll()
    {
        if (UpgradeManager.Instance.rerollsAvailable <= 0) return;
        UpgradeManager.Instance.rerollsAvailable--;
        List<UpgradeData> options = UpgradeManager.Instance.GetRandomOptions();
        currentOptions = options;
        for (int i = 0; i < optionSlots.Length; i++)
            optionSlots[i].Setup(i < options.Count ? options[i] : null, i);
        selectedIndex = 0;
        RefreshRerollButton();
        FocusSelected();
    }

    void RefreshRerollButton()
    {
        if (rerollButton == null) return;
        int rerolls = UpgradeManager.Instance != null ? UpgradeManager.Instance.rerollsAvailable : 0;
        rerollButton.SetActive(rerolls > 0);
        if (rerollButtonText != null)
            rerollButtonText.text = $"Reroll ({rerolls})";
    }

    [Header("Slide-in")]
    [SerializeField] float staggerDelay = 0.08f;

    IEnumerator PreGraceCoroutine()
    {
        BeatConductor.Instance.FadeParameter("Pause Menu", 1f, preUIGraceSeconds);
        yield return new WaitForSecondsRealtime(preUIGraceSeconds);
        selectedIndex = 0;
        panel.SetActive(true);
        RefreshRerollButton();
        FocusSelected();

        for (int i = 0; i < optionSlots.Length; i++)
        {
            if (optionSlots[i].gameObject.activeSelf)
                StartCoroutine(optionSlots[i].SlideIn(i * staggerDelay));
        }
    }

    // Grid layout:  TL=0  TR=3
    //               BL=1  BR=2
    // [slotIndex][direction] -> neighbor index (0=up, 1=down, 2=left, 3=right)
    static readonly int[][] navTable = {
        new[] { 1, 1, 3, 3 }, // 0 TL
        new[] { 0, 0, 2, 2 }, // 1 BL
        new[] { 3, 3, 1, 1 }, // 2 BR
        new[] { 2, 2, 0, 0 }, // 3 TR
    };

    void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            NavigateGrid(0);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            NavigateGrid(1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            NavigateGrid(2);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            NavigateGrid(3);

        if (Input.GetKeyDown(KeyCode.E))
            optionSlots[selectedIndex].Select();
    }

    public void SetSelected(int index)
    {
        optionSlots[selectedIndex].SetHighlighted(false);
        selectedIndex = index;
        FocusSelected();
    }

    void NavigateGrid(int dir)
    {
        SetSelected(navTable[selectedIndex][dir]);
    }

    void FocusSelected()
    {
        foreach (var slot in optionSlots)
            slot.SetHighlighted(false);
        EventSystem.current.SetSelectedGameObject(optionSlots[selectedIndex].button.gameObject);
        optionSlots[selectedIndex].SetHighlighted(true);
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        UpgradeManager.Instance.Apply(upgrade);
        panel.SetActive(false);
        if (!pendingWeaponSelect)
            StartCoroutine(PostGraceCoroutine());
    }

    public void SetPendingWeaponSelect(bool pending) => pendingWeaponSelect = pending;

    public void ResumeAfterSelect()
    {
        pendingWeaponSelect = false;
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
