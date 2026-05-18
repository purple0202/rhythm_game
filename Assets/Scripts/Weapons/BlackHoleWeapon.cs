using UnityEngine;
using System.Collections.Generic;

public class BlackHoleWeapon : Weapon
{
    [Header("Damage")]
    public float baseDamage    = 15f;
    public float damagePerBeat = 10f;
    public float autoDamage    = 8f;

    [Header("Charge")]
    public int   maxChargeLevel      = 5;
    public float baseProjectileScale = 0.3f;
    public float scalePerBeat        = 0.2f;
    public float previewDistance     = 1f;

    [Header("Projectile")]
    public float projectileSpeed = 10f;
    public float projectileRange = 8f;

    [Header("Black Hole")]
    public float blackHoleDamagePerBeat = 5f;
    public int   blackHoleLifetimeBeats = 4;
    public float blackHolePullForce     = 3f;

    [Header("Timing Bonus")]
    public float perfectPressBonus   = 1.2f;
    public float perfectReleaseBonus = 1.2f;

    [Header("References")]
    public GameObject projectilePrefab;
    public GameObject autoProjectilePrefab;
    public GameObject blackHolePrefab;

    bool                isCharging;
    int                 chargeLevel;
    float               storedDamage;
    float               pressMultiplier = 1f;
    float               storedPassiveBonus;
    Vector2             aimDirection    = Vector2.right;
    Transform           playerTr;
    BlackHoleProjectile previewInstance;

    public override bool IsAttacking => isCharging;

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? baseDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? baseDamage + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage + AutoDamageBonus : 0f;

    void OnEnable()
    {
        playerTr = GetComponentInParent<PlayerMovement>()?.transform ?? transform;
    }

    void OnDisable()
    {
        if (isCharging) CancelCharge();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(x, y);
        if (input.sqrMagnitude > 0.01f)
            aimDirection = input.normalized;

        if (!isCharging) return;

        if (previewInstance != null && playerTr != null)
            previewInstance.transform.position = (Vector2)playerTr.position + aimDirection * previewDistance;

        if (!Input.GetKey(KeyCode.P) && !Input.GetMouseButton(0))
            FireProjectile();
    }

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Auto")
        {
            if (!isCharging) FireAutoProjectile();
            return;
        }

        if (isCharging || projectilePrefab == null) return;

        isCharging   = true;
        chargeLevel  = 1;
        storedDamage = judgement == "Perfect"
            ? baseDamage + PerfectDamageBonus + ActiveDamageBonus
            : baseDamage + GoodDamageBonus    + ActiveDamageBonus;
        pressMultiplier    = judgement == "Perfect" ? perfectPressBonus : 1f;
        storedPassiveBonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;

        Vector2 spawnPos = playerTr != null
            ? (Vector2)playerTr.position + aimDirection * previewDistance
            : (Vector2)transform.position;

        GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * baseProjectileScale;
        previewInstance = go.GetComponent<BlackHoleProjectile>();

        BeatConductor.OnBeat += OnBeat;
    }

    void OnBeat()
    {
        if (Time.timeScale == 0) return;
        if (chargeLevel < maxChargeLevel) chargeLevel++;

        float scale = baseProjectileScale + (chargeLevel - 1) * scalePerBeat;
        if (previewInstance != null)
            previewInstance.transform.localScale = Vector3.one * scale;
    }

    void FireProjectile()
    {
        BeatConductor.OnBeat -= OnBeat;
        isCharging = false;

        if (previewInstance == null) return;

        float spb          = BeatConductor.Instance.secondsPerBeat;
        float beatPhase    = Mathf.Repeat(BeatConductor.Instance.songPosition - BeatConductor.Instance.lastBeatTime, spb);
        float closest      = Mathf.Min(beatPhase, spb - beatPhase);
        float releaseBonus = closest <= 0.05f ? perfectReleaseBonus : 1f;

        float comboMult   = ComboSystem.Instance    != null ? ComboSystem.Instance.GetCurrentMultiplier()    : 1f;
        float passiveMult = PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier()  : 1f;
        float damage      = (storedDamage + (chargeLevel - 1) * damagePerBeat + storedPassiveBonus)
                          * pressMultiplier * releaseBonus * comboMult * passiveMult;

        previewInstance.damage                 = damage;
        previewInstance.speed                  = projectileSpeed;
        previewInstance.lifetime               = projectileRange / projectileSpeed;
        previewInstance.weaponType             = weaponType;
        previewInstance.dotApplications        = new List<DotType>(dotApplications);
        previewInstance.isMaxCharge            = chargeLevel >= maxChargeLevel;
        previewInstance.blackHolePrefab        = blackHolePrefab;
        previewInstance.blackHoleDamagePerBeat = blackHoleDamagePerBeat;
        previewInstance.blackHoleLifetimeBeats = blackHoleLifetimeBeats;
        previewInstance.blackHolePullForce     = blackHolePullForce;

        previewInstance.Launch(aimDirection);
        previewInstance = null;
    }

    void FireAutoProjectile()
    {
        GameObject prefab = autoProjectilePrefab != null ? autoProjectilePrefab : projectilePrefab;
        if (prefab == null) return;

        float passiveBonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus   : 0f;
        float comboMult    = ComboSystem.Instance    != null ? ComboSystem.Instance.GetCurrentMultiplier()   : 1f;
        float passiveMult  = PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f;
        float damage       = (autoDamage + AutoDamageBonus + passiveBonus) * comboMult * passiveMult;

        Vector2 spawnPos = playerTr != null ? (Vector2)playerTr.position : (Vector2)transform.position;

        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * baseProjectileScale;

        BlackHoleProjectile proj = go.GetComponent<BlackHoleProjectile>();
        if (proj == null) return;

        proj.damage          = damage;
        proj.speed           = projectileSpeed;
        proj.lifetime        = projectileRange / projectileSpeed;
        proj.weaponType      = weaponType;
        proj.dotApplications = new List<DotType>(dotApplications);
        proj.isMaxCharge     = false;
        proj.Launch(aimDirection);
    }

    void CancelCharge()
    {
        isCharging      = false;
        chargeLevel     = 0;
        pressMultiplier = 1f;
        BeatConductor.OnBeat -= OnBeat;

        if (previewInstance != null)
        {
            Destroy(previewInstance.gameObject);
            previewInstance = null;
        }
    }
}
