using UnityEngine;

[CreateAssetMenu(menuName = "Game/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public string description;
    public Sprite icon;

    [Header("Conditions")]
    public bool requiresWeapon;
    public string requiredWeaponName;

    [Header("Effects")]
    public float damageMultiplier = 1f;
    public float moveSpeedMultiplier = 1f;
    public int extraProjectiles = 0;

    public bool CanApply(PlayerStats player)
    {
        if (requiresWeapon)
        {
            if (!player.HasWeapon(requiredWeaponName))
                return false;
        }

        return true;
    }

    public void Apply(PlayerStats player)
    {
        player.damage *= damageMultiplier;
        player.moveSpeed *= moveSpeedMultiplier;
        player.projectileCount += extraProjectiles;
    }
}