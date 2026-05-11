using UnityEngine;

public class DistortionEffect : UpgradeEffect
{
    public override bool IsStackable => false;

    public override void Apply()
    {
        WeaponController wc = FindFirstObjectByType<WeaponController>();
        if (wc == null || wc.EquippedWeapons.Count == 0) return;

        if (wc.EquippedWeapons.Count == 1)
        {
            if (!wc.EquippedWeapons[0].dotApplications.Contains(DotType.Electric))
                wc.EquippedWeapons[0].dotApplications.Add(DotType.Electric);
            return;
        }

        UpgradeUI.Instance.SetPendingWeaponSelect(true);
        WeaponDotSelectUI.Instance.Show(DotType.Electric, wc.EquippedWeapons);
    }
}
