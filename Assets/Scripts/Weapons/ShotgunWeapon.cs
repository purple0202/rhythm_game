using UnityEngine;
using System.Collections.Generic;

public class ShotgunWeapon : Weapon
{
    [Header("Main Projectile Damage")]
    public float perfectMainDamage = 40f;
    public float goodMainDamage = 25f;
    public float autoMainDamage = 15f;

    [Header("Sub Projectile Damage")]
    public float perfectSubDamage = 15f;
    public float goodSubDamage = 10f;
    public float autoSubDamage = 6f;

    [Header("Spread")]
    public float innerAngle = 12f;
    public float outerAngle = 24f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    [Tooltip("Scale of sub-pellets relative to main pellet (e.g. 0.6 = 60% size)")]
    public float subProjectileScale = 0.6f;
    [Tooltip("How long each pellet lives (seconds). Lower = shorter range. Overrides the prefab value.")]
    public float pelletLifetime = 0.4f;

    private Vector2 aimDirection = Vector2.right;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(x, y);
        if (input.sqrMagnitude > 0.01f)
            aimDirection = input.normalized;
    }

    public override float GetAutoDamage() => autoMainDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? perfectMainDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? goodMainDamage    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoMainDamage    + AutoDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Bad") return;
        if (projectilePrefab == null) return;

        float mainDmg, subDmg;

        if (judgement == "Perfect")
        {
            mainDmg = perfectMainDamage + PerfectDamageBonus + ActiveDamageBonus;
            subDmg  = perfectSubDamage  + PerfectDamageBonus + ActiveDamageBonus;
        }
        else if (judgement == "Good")
        {
            mainDmg = goodMainDamage + GoodDamageBonus + ActiveDamageBonus;
            subDmg  = goodSubDamage  + GoodDamageBonus + ActiveDamageBonus;
        }
        else // Auto
        {
            mainDmg = autoMainDamage + AutoDamageBonus;
            subDmg  = autoSubDamage  + AutoDamageBonus;
        }

        float passiveBonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;
        mainDmg += passiveBonus;
        subDmg  += passiveBonus;

        float mainScale = 1f + SizeBonus * 0.1f;
        float subScale  = mainScale * subProjectileScale;

        // Center (main)
        SpawnPellet(aimDirection, 0f, mainDmg, mainScale);

        // Inner sub-pellets (auto and manual)
        SpawnPellet(aimDirection,  innerAngle, subDmg, subScale);
        SpawnPellet(aimDirection, -innerAngle, subDmg, subScale);

        // Outer sub-pellets (manual only)
        if (judgement != "Auto")
        {
            SpawnPellet(aimDirection,  outerAngle, subDmg, subScale);
            SpawnPellet(aimDirection, -outerAngle, subDmg, subScale);
        }
    }

    private void SpawnPellet(Vector2 baseDir, float angleDelta, float damage, float scale)
    {
        Vector2 dir = Rotate(baseDir, angleDelta);
        Vector3 spawnPos = GetComponentInParent<PlayerMovement>()?.transform.position ?? transform.position;

        GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile p = go.GetComponent<Projectile>();
        if (p != null)
        {
            p.weaponType = weaponType;
            p.dotApplications = new List<DotType>(dotApplications);
            p.SetDirection(dir);
            p.damage = damage;
            p.lifetime = pelletLifetime;
        }
        go.transform.localScale = Vector3.one * scale;
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
