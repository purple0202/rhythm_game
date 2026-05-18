using UnityEngine;
using System.Collections.Generic;

public class LandMineWeapon : Weapon
{
    [Header("Damage")]
    public float perfectDamage = 30f;
    public float goodDamage    = 20f;
    public float autoDamage    = 10f;

    [Header("Placement")]
    public float fixedRadius    = 3f;
    public float detectionRange = 12f;

    [Header("Mine Settings")]
    public float explosionRadius = 3f;
    public int   lifetimeBeats   = 4;
    public int   maxMines        = 20;

    [Header("Prefab")]
    public GameObject landMinePrefab;

    readonly List<LandMine> activeMines = new();

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? goodDamage    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage    + AutoDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Bad" || landMinePrefab == null) return;

        float damage = judgement == "Perfect" ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus :
                       judgement == "Good"    ? goodDamage    + GoodDamageBonus    + ActiveDamageBonus :
                                               autoDamage     + AutoDamageBonus;

        float multiplier = (ComboSystem.Instance   != null ? ComboSystem.Instance.GetCurrentMultiplier()    : 1f)
                         * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f);
        float bonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;

        Transform playerTr = GetComponentInParent<PlayerMovement>()?.transform ?? transform;
        Vector2   dir      = GetAimDirection(playerTr.position);
        Vector2   target   = (Vector2)playerTr.position + dir * fixedRadius;

        activeMines.RemoveAll(m => m == null);
        if (activeMines.Count >= maxMines)
        {
            Destroy(activeMines[0].gameObject);
            activeMines.RemoveAt(0);
        }

        GameObject go   = Instantiate(landMinePrefab, playerTr.position, Quaternion.identity);
        LandMine   mine = go.GetComponent<LandMine>();
        if (mine != null)
        {
            mine.damage          = (damage + bonus) * multiplier;
            mine.weaponType      = weaponType;
            mine.dotApplications = new List<DotType>(dotApplications);
            mine.target          = target;
            mine.explosionRadius = explosionRadius * (1f + SizeBonus * 0.1f);
            mine.lifetimeBeats   = lifetimeBeats;
            activeMines.Add(mine);
        }
    }

    Vector2 GetAimDirection(Vector2 from)
    {
        Collider2D[] hits    = Physics2D.OverlapCircleAll(from, detectionRange);
        float        minDist = float.MaxValue;
        Vector2      best    = Vector2.right;
        bool         found   = false;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            float dist = Vector2.Distance(from, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                best    = ((Vector2)hit.transform.position - from).normalized;
                found   = true;
            }
        }

        if (!found)
        {
            PlayerMovement pm = GetComponentInParent<PlayerMovement>();
            best = Vector2.right * (pm != null ? pm.facingDirection : 1);
        }

        return best;
    }
}
