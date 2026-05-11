using UnityEngine;

public class PracticePracticePracticeEffect : UpgradeEffect
{
    public override bool IsStackable => false;

    public override void Apply()
    {
        PlayerStats s = PlayerStats.Instance;
        if (s == null) return;

        s.bonusAutoDamage        *= 2f;
        s.bonusActiveDamage      *= 2f;
        s.bonusPerfectDamage     *= 2f;
        s.bonusGoodDamage        *= 2f;
        s.bonusMoveSpeed         *= 2f;
        s.bonusMaxHealth         *= 2f;
        s.weaponSizeBonus        *= 2f;
        s.damageReductionPercent *= 2f;
        s.projectileBonus        *= 2;
        s.dashCooldownReduction  *= 2f;
    }
}
