using UnityEngine;

public class PopularityEffect : UpgradeEffect
{
    public override void Apply()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.enemySpawnMultiplier += 0.1f;
    }
}
