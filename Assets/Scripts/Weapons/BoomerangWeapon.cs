using UnityEngine;
using System.Collections.Generic;

public class BoomerangWeapon : Weapon
{
    [Header("Damage - Outbound")]
    public float perfectOutDamage = 20f;
    public float goodOutDamage    = 14f;
    public float autoOutDamage    = 7f;

    [Header("Damage - Returning")]
    public float perfectReturnDamage = 15f;
    public float goodReturnDamage    = 10f;
    public float autoReturnDamage    = 5f;

    [Header("Settings")]
    public float maxRange          = 8f;
    public float detectionRange    = 12f;
    public float speed             = 10f;
    public float returnSpeed       = 12f;
    public bool  allowDoubleDamage = true;

    [Header("Prefab")]
    public GameObject boomerangPrefab;

    public override float GetAutoDamage() => autoOutDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? perfectOutDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? goodOutDamage    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoOutDamage    + AutoDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Bad" || boomerangPrefab == null) return;

        float multiplier = (ComboSystem.Instance   != null ? ComboSystem.Instance.GetCurrentMultiplier()     : 1f)
                         * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier()  : 1f);
        float bonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;

        float outDmg = judgement == "Perfect" ? perfectOutDamage + PerfectDamageBonus + ActiveDamageBonus :
                       judgement == "Good"    ? goodOutDamage    + GoodDamageBonus    + ActiveDamageBonus :
                                               autoOutDamage     + AutoDamageBonus;

        float retDmg = judgement == "Perfect" ? perfectReturnDamage :
                       judgement == "Good"    ? goodReturnDamage    :
                                               autoReturnDamage;

        float sizeMult     = 1f + SizeBonus * 0.1f;
        Transform playerTr = GetComponentInParent<PlayerMovement>()?.transform ?? transform;
        Vector2 dir        = GetFireDirection(playerTr.position);

        GameObject go = Instantiate(boomerangPrefab, playerTr.position, Quaternion.identity);
        var b = go.GetComponent<Boomerang>();
        if (b != null)
        {
            b.outboundDamage    = (outDmg + bonus) * multiplier;
            b.returnDamage      = retDmg * multiplier;
            b.maxRange          = maxRange * sizeMult;
            b.speed             = speed;
            b.returnSpeed       = returnSpeed;
            b.allowDoubleDamage = allowDoubleDamage;
            b.direction         = dir;
            b.weaponType        = weaponType;
            b.dotApplications   = new List<DotType>(dotApplications);
            b.playerTransform   = playerTr;
        }
    }

    Vector2 GetFireDirection(Vector2 from)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(from, detectionRange);
        float   minDist = float.MaxValue;
        Vector2 best    = Vector2.right;
        bool    found   = false;

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
