using UnityEngine;

public class JukeboxEffect : UpgradeEffect
{
    public override bool IsStackable =>
        UpgradeManager.Instance == null || UpgradeManager.Instance.totalRerolls < 3;

    public override void Apply()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.totalRerolls =
                Mathf.Min(3, UpgradeManager.Instance.totalRerolls + 1);
    }
}
