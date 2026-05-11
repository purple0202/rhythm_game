using UnityEngine;

public class LargerAmpEffect : UpgradeEffect
{
    public float bonusPerStack = 1f;
    public int   maxStacks     = 20;

    public override bool IsStackable =>
        data == null || UpgradeManager.Instance == null ||
        UpgradeManager.Instance.GetAppliedCount(data) < maxStacks;

    public override void Apply()
    {
        if (PlayerStats.Instance == null) return;
        int applied = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetAppliedCount(data) : 0;
        if (applied < maxStacks)
            PlayerStats.Instance.weaponSizeBonus += bonusPerStack;
    }
}
