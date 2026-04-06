using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public List<UpgradeData> allUpgrades;

    public int optionsToShow = 4;

    void Awake()
    {
        Instance = this;
    }

    public List<UpgradeData> GetAvailableUpgrades(PlayerStats player)
    {
        return allUpgrades
            .Where(upgrade => upgrade.CanApply(player))
            .ToList();
    }

    public List<UpgradeData> GetRandomUpgrades(PlayerStats player)
    {
        List<UpgradeData> valid = GetAvailableUpgrades(player);

        return valid
            .OrderBy(x => Random.value)
            .Take(optionsToShow)
            .ToList();
    }
}