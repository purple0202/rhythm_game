using System.Collections.Generic;
using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance;

    public GameObject panel;

    public UpgradeOptionUI[] optionSlots;

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
        {
            optionSlots[i].Setup(upgrades[i]);
        }
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        Debug.Log("upgrade selected!");

        upgrade.Apply(PlayerStats.Instance);

        panel.SetActive(false);

        Time.timeScale = 1f;
    }
}