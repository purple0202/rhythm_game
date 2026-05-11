using UnityEngine;

public class BatonEffect : UpgradeEffect
{
    public override void Apply()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.bonusPerfectDamage += 1f;
    }
}
