using UnityEngine;

public class MetronomeEffect : UpgradeEffect
{
    public float bonusPerStack = 1f;

    public override bool IsStackable => true;

    public override void Apply()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.bonusAutoDamage += bonusPerStack;
    }
}
