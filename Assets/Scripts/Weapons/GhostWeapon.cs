using UnityEngine;
using System.Collections.Generic;

public class GhostWeapon : Weapon
{
    public GameObject ghostPrefab;
    public GameObject projectilePrefab;
    public float searchRadius = 20f;

    [Header("Damage")]
    public float autoDamage    = 8f;
    public float goodBonus     = 5f;
    public float perfectBonus  = 10f;

    [Header("Hover")]
    public Vector2 hoverOffset  = new Vector2(1f, 1f);
    public float followSpeed    = 5f;
    public float bobSpeed       = 2f;
    public float bobAmount      = 0.2f;

    public override bool IsPassive => true;

    private GhostCompanion _ghost;

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? autoDamage + perfectBonus + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? autoDamage + goodBonus    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage + AutoDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (_ghost == null) SpawnGhost();
        if (judgement == "Bad") return;

        float bonus = judgement switch
        {
            "Perfect" => perfectBonus + PerfectDamageBonus + ActiveDamageBonus
                       + (PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f),
            "Good"    => goodBonus    + GoodDamageBonus    + ActiveDamageBonus
                       + (PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f),
            _         => 0f,
        };

        if (bonus > 0f) _ghost.SetBonus(bonus);
    }

    void SpawnGhost()
    {
        Transform playerTr = GetComponentInParent<PlayerMovement>()?.transform ?? transform;
        GameObject go = Instantiate(ghostPrefab, playerTr.position, Quaternion.identity);
        _ghost = go.GetComponent<GhostCompanion>();
        _ghost.Initialize(playerTr, hoverOffset, autoDamage + AutoDamageBonus,
                          followSpeed, bobSpeed, bobAmount, searchRadius,
                          projectilePrefab, weaponType, new List<DotType>(dotApplications));
    }

    void OnDisable()
    {
        if (_ghost != null) { Destroy(_ghost.gameObject); _ghost = null; }
    }
}
