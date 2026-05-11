using UnityEngine;
using System.Collections.Generic;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;
    [TextArea] public string description;
    public Sprite icon;
    public EnemyType weaponType = EnemyType.None;

    public abstract void PerformAttack(string judgement);

    public virtual float GetAutoDamage() => 0f;
    public virtual float GetDamageForJudgement(string judgement) => 0f;

    public virtual bool IsAttacking => false;

    // Populated by Distortion / Fireworks upgrades.
    public List<DotType> dotApplications = new();

    // Shorthand helpers so subclasses don't repeat null-checks.
    protected float ActiveDamageBonus  => PlayerStats.Instance != null ? PlayerStats.Instance.bonusActiveDamage  : 0f;
    protected float PerfectDamageBonus => PlayerStats.Instance != null ? PlayerStats.Instance.bonusPerfectDamage : 0f;
    protected float GoodDamageBonus    => PlayerStats.Instance != null ? PlayerStats.Instance.bonusGoodDamage    : 0f;
    protected float AutoDamageBonus    => PlayerStats.Instance != null ? PlayerStats.Instance.bonusAutoDamage    : 0f;
    protected float SizeBonus          => PlayerStats.Instance != null ? PlayerStats.Instance.weaponSizeBonus    : 0f;

    protected void ApplyDots(GameObject enemyGO)
    {
        if (dotApplications.Count == 0) return;
        DotReceiver dr = enemyGO.GetComponent<DotReceiver>();
        if (dr == null) return;
        foreach (var dot in dotApplications) dr.Apply(dot);
    }
}