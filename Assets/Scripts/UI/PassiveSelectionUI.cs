using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PassiveSelectionUI : MonoBehaviour
{
    public static PassiveSelectionUI Instance;

    public GameObject panel;
    public PassiveOptionUI[] optionSlots;

    [Header("Grace Periods")]
    public float preUIGraceSeconds = 1f;
    public int postUIGraceBeats = 1;

    int selectedIndex;
    int visibleSlotCount;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
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
        PassiveManager.Instance.Equip(data);
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
