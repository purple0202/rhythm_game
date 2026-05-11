using UnityEngine;

public class FireworksEffect : UpgradeEffect
{
    public override bool IsStackable => false;

    public override void Apply()
    {
        WeaponController wc = FindFirstObjectByType<WeaponController>();
        if (wc == null || wc.EquippedWeapons.Count == 0) return;

        if (wc.EquippedWeapons.Count == 1)
        {
            if (!wc.EquippedWeapons[0].dotApplications.Contains(DotType.Fire))
                wc.EquippedWeapons[0].dotApplications.Add(DotType.Fire);
            return;
        }

        UpgradeUI.Instance.SetPendingWeaponSelect(true);
        WeaponDotSelectUI.Instance.Show(DotType.Fire, wc.EquippedWeapons);
    }
}
