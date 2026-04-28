using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance;

    public GameObject panel;
    public UpgradeOptionUI[] optionSlots;

    [Header("Grace Periods")]
    public float preUIGraceSeconds = 1f;
    public int postUIGraceBeats = 1;

    int selectedIndex;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show()
    {
        List<UpgradeData> upgrades =
            UpgradeManager.Instance.GetRandomUpgrades(PlayerStats.Instance);

        for (int i = 0; i < optionSlots.Length; i++)
            optionSlots[i].Setup(upgrades[i], i);

        StartCoroutine(PreGraceCoroutine());
    }

    IEnumerator PreGraceCoroutine()
    {
        yield return new WaitForSecondsRealtime(preUIGraceSeconds);
        selectedIndex = 0;
        panel.SetActive(true);
        FocusSelected();
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
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        upgrade.Apply(PlayerStats.Instance);
        panel.SetActive(false);
        StartCoroutine(PostGraceCoroutine());
    }

    IEnumerator PostGraceCoroutine()
    {
        int beatsRemaining = postUIGraceBeats;
        System.Action onBeat = () => beatsRemaining--;
        BeatConductor.OnBeat += onBeat;
        yield return new WaitUntil(() => beatsRemaining <= 0);
        BeatConductor.OnBeat -= onBeat;
        Time.timeScale = 1f;
    }
}
