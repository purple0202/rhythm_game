using UnityEngine;

public class UptempoEffect : UpgradeEffect
{
    public float bonusPerStack = 0.3f;
    public float maxBonus      = 1.5f;

    public override bool IsStackable =>
        data == null || UpgradeManager.Instance == null ||
        UpgradeManager.Instance.GetAppliedCount(data) * bonusPerStack < maxBonus;

    public override void Apply()
    {
        if (PlayerStats.Instance == null) return;
        float alreadyApplied = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.GetAppliedCount(data) * bonusPerStack
            : 0f;
        float add = Mathf.Min(bonusPerStack, maxBonus - alreadyApplied);
        PlayerStats.Instance.bonusMoveSpeed += add;
    }
}
