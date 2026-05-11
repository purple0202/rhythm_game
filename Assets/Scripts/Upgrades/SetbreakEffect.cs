using UnityEngine;

public class SetbreakEffect : UpgradeEffect
{
    public override void Apply()
    {
        PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
            ph.Heal(ph.maxHealth * 0.25f);
    }
}
