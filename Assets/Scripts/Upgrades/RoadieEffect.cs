using UnityEngine;

public class RoadieEffect : UpgradeEffect
{
    public float cooldownReduction = 0.5f;

    public override void Apply()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.dashCooldownReduction += cooldownReduction;
    }
}
