using UnityEngine;
using System.Collections.Generic;

public class OrbitalWeapon : Weapon
{
    public GameObject orbitalPrefab;
    public float orbitRadius = 2f;
    public float hitCooldown = 0.4f;

    [Header("Damage")]
    public float autoDamage    = 5f;
    public float goodDamage    = 8f;
    public float perfectDamage = 12f;

    [Header("Orbit Sweep Speed")]
    public float orbitRotationSpeed = 360f;

    [Header("Self Spin Speed")]
    public float selfRotationSpeed = 180f;

    [Header("Degrees Added per Input")]
    public float perfectDegrees = 90f;
    public float goodDegrees    = 60f;
    public float badDegrees     = 15f;
    public float autoDegrees    = 30f;

    private OrbitalObject _orbital;

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? goodDamage    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage    + AutoDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (_orbital == null)
            SpawnOrbital();

        _orbital.damage = judgement switch
        {
            "Perfect" => perfectDamage + PerfectDamageBonus + ActiveDamageBonus,
            "Good"    => goodDamage    + GoodDamageBonus    + ActiveDamageBonus,
            _         => autoDamage    + AutoDamageBonus,
        };

        _orbital.RotateBy(judgement switch
        {
            "Perfect" => perfectDegrees,
            "Good"    => goodDegrees,
            "Bad"     => badDegrees,
            _         => autoDegrees,
        });
    }

    void SpawnOrbital()
    {
        Transform playerTr = GetComponentInParent<PlayerMovement>()?.transform ?? transform;
        float sizeMult = 1f + SizeBonus * 0.1f;

        GameObject go = Instantiate(orbitalPrefab, playerTr.position, Quaternion.identity);
        if (sizeMult != 1f) go.transform.localScale = Vector3.one * sizeMult;

        _orbital = go.GetComponent<OrbitalObject>();
        _orbital.Initialize(playerTr, orbitRadius, autoDamage + AutoDamageBonus,
                            hitCooldown, orbitRotationSpeed, selfRotationSpeed,
                            weaponType, new List<DotType>(dotApplications));
    }

    void OnDisable()
    {
        if (_orbital != null)
        {
            Destroy(_orbital.gameObject);
            _orbital = null;
        }
    }
}
