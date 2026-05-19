using UnityEngine;

public class RicochetWeapon : Weapon
{
    public GameObject projectilePrefab;

    public float firstHitDamage = 12f;
    public float bounceDamage = 8f;
    public float autoDamage = 6f;
    public int maxBounces = 3;
    public float chainRange = 8f;
    public float travelSpeed = 14f;

    [Header("Perfect Bonuses")]
    public int perfectBonusBounces = 2;
    public float perfectBonusFirstHitDamage = 5f;
    public float perfectBonusBounceDamage = 3f;

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j)
    {
        if (j == "Bad") return 0f;
        float bonus = j == "Auto" ? AutoDamageBonus
                    : ActiveDamageBonus + (j == "Perfect" ? PerfectDamageBonus : GoodDamageBonus);
        return firstHitDamage + bonus;
    }

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Bad") return;

        Transform target = FindClosestEnemy(transform.position, Mathf.Infinity);
        if (target == null) return;

        float statBonus = judgement == "Auto"
            ? AutoDamageBonus
            : ActiveDamageBonus + (judgement == "Perfect" ? PerfectDamageBonus : GoodDamageBonus);

        float pendingBonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;
        float passiveMult  = PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f;
        float comboMult    = ComboSystem.Instance != null ? ComboSystem.Instance.GetCurrentMultiplier() : 1f;
        float totalMult    = comboMult * passiveMult;

        bool isAuto    = judgement == "Auto";
        bool isPerfect = judgement == "Perfect";
        float fHit  = isAuto ? autoDamage + AutoDamageBonus
                             : firstHitDamage + statBonus + pendingBonus + (isPerfect ? perfectBonusFirstHitDamage : 0f);
        float bHit  = isAuto ? autoDamage + AutoDamageBonus
                             : bounceDamage + statBonus + pendingBonus + (isPerfect ? perfectBonusBounceDamage : 0f);
        int bounces = maxBounces + (isPerfect ? perfectBonusBounces : 0);

        GameObject go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        if (SizeBonus > 0f)
            go.transform.localScale = Vector3.one * (1f + SizeBonus * 0.1f);

        RicochetProjectile rp = go.GetComponent<RicochetProjectile>();
        rp.Initialize(target, fHit, bHit, totalMult, bounces, chainRange, travelSpeed,
                      weaponType, new System.Collections.Generic.List<DotType>(dotApplications));
    }

    public static Transform FindClosestEnemy(Vector2 origin, float range)
    {
        Collider2D[] hits = range >= 9999f
            ? Physics2D.OverlapCircleAll(origin, 9999f)
            : Physics2D.OverlapCircleAll(origin, range);

        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            float d = Vector2.Distance(origin, hit.transform.position);
            if (d < minDist) { minDist = d; closest = hit.transform; }
        }
        return closest;
    }
}
