using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance;

    public GameObject panel;
    public UpgradeOptionUI[] optionSlots;

    int selectedIndex;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);

        List<UpgradeData> upgrades =
            UpgradeManager.Instance.GetRandomUpgrades(PlayerStats.Instance);

        for (int i = 0; i < optionSlots.Length; i++)
            optionSlots[i].Setup(upgrades[i]);

        selectedIndex = 0;
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

    void Navigate(int dir)
    {
        optionSlots[selectedIndex].SetHighlighted(false);
        selectedIndex = (selectedIndex + dir + optionSlots.Length) % optionSlots.Length;
        FocusSelected();
    }

    void FocusSelected()
    {
        EventSystem.current.SetSelectedGameObject(optionSlots[selectedIndex].button.gameObject);
        optionSlots[selectedIndex].SetHighlighted(true);
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        upgrade.Apply(PlayerStats.Instance);
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}