using UnityEngine;

public class FineTuningEffect : UpgradeEffect
{
    public float bonusPerStack = 1f;

    public override bool IsStackable => true;

    public override void Apply()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.bonusActiveDamage += bonusPerStack;
    }
}
