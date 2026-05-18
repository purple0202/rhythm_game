using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThrowDaggerWeapon : Weapon
{
    [Header("Damage")]
    public float perfectDamage = 25f;
    public float goodDamage    = 18f;
    public float autoDamage    = 8f;

    [Header("Volley")]
    public int   maxDaggers         = 5;
    public int   maxSpread          = 3;
    public float spreadSpacing      = 0.4f;
    public float timeBetweenDaggers = 0.08f;
    public float daggerSpeed        = 15f;
    public float volleyRange        = 8f;

    [Header("Auto Dagger")]
    public float autoSpeed = 10f;
    public float autoRange = 4f;

    [Header("Timing Bonus")]
    public float perfectPressBonus   = 1.2f;
    public float perfectReleaseBonus = 1.2f;

    [Header("Prefab")]
    public GameObject daggerPrefab;

    bool    isCharging;
    int     daggerCount;
    float   storedDamage;
    float   pressMultiplier   = 1f;
    float   storedPassiveBonus;
    Vector2 aimDirection      = Vector2.right;
    Coroutine volleyCoroutine;

    public override bool IsAttacking => isCharging;

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? goodDamage    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage    + AutoDamageBonus : 0f;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(x, y);
        if (input.sqrMagnitude > 0.01f)
            aimDirection = input.normalized;

        if (!isCharging) return;
        if (!Input.GetKey(KeyCode.P) && !Input.GetMouseButton(0))
            FireVolley();
    }

    void OnDisable()
    {
        if (isCharging) CancelCharge();
        if (volleyCoroutine != null)
        {
            StopCoroutine(volleyCoroutine);
            volleyCoroutine = null;
        }
    }

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Auto")
        {
            if (!isCharging) FireAutoDagger();
            return;
        }

        if (isCharging) return;

        isCharging    = true;
        daggerCount   = 1;
        storedDamage  = judgement == "Perfect"
            ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus
            : goodDamage    + GoodDamageBonus    + ActiveDamageBonus;
        pressMultiplier    = judgement == "Perfect" ? perfectPressBonus : 1f;
        storedPassiveBonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;

        BeatConductor.OnBeat += OnBeat;
    }

    void OnBeat()
    {
        if (Time.timeScale == 0) return;
        if (daggerCount < maxDaggers) daggerCount++;
    }

    void FireVolley()
    {
        BeatConductor.OnBeat -= OnBeat;
        isCharging = false;

        if (daggerPrefab == null) return;

        float spb          = BeatConductor.Instance.secondsPerBeat;
        float beatPhase    = Mathf.Repeat(BeatConductor.Instance.songPosition - BeatConductor.Instance.lastBeatTime, spb);
        float closest      = Mathf.Min(beatPhase, spb - beatPhase);
        float releaseBonus = closest <= 0.05f ? perfectReleaseBonus : 1f;

        float comboMult   = ComboSystem.Instance    != null ? ComboSystem.Instance.GetCurrentMultiplier()    : 1f;
        float passiveMult = PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier()  : 1f;
        float damage      = (storedDamage + storedPassiveBonus) * pressMultiplier * releaseBonus * comboMult * passiveMult;
        float lifetime    = volleyRange / daggerSpeed;

        volleyCoroutine = StartCoroutine(FireVolleyCoroutine(daggerCount, damage, lifetime, aimDirection));
    }

    IEnumerator FireVolleyCoroutine(int count, float damage, float lifetime, Vector2 dir)
    {
        Transform playerTr = GetComponentInParent<PlayerMovement>()?.transform ?? transform;
        Vector2   perp     = new Vector2(-dir.y, dir.x);
        float     angle    = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            if (playerTr == null) yield break;

            int row        = i / maxSpread;
            int posInRow   = i % maxSpread;
            int remaining  = count - row * maxSpread;
            int countInRow = Mathf.Min(remaining, maxSpread);

            float   perpOffset = (posInRow - (countInRow - 1) / 2f) * spreadSpacing;
            Vector2 spawnPos   = (Vector2)playerTr.position + perp * perpOffset;

            GameObject go = Instantiate(daggerPrefab, spawnPos, Quaternion.AngleAxis(angle, Vector3.forward));
            Dagger     d  = go.GetComponent<Dagger>();
            if (d != null)
            {
                d.damage          = damage;
                d.speed           = daggerSpeed;
                d.lifetime        = lifetime;
                d.weaponType      = weaponType;
                d.dotApplications = new List<DotType>(dotApplications);
                d.direction       = dir;
            }

            yield return new WaitForSeconds(timeBetweenDaggers);
        }

        volleyCoroutine = null;
    }

    void FireAutoDagger()
    {
        if (daggerPrefab == null) return;

        float passiveBonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus   : 0f;
        float comboMult    = ComboSystem.Instance    != null ? ComboSystem.Instance.GetCurrentMultiplier()   : 1f;
        float passiveMult  = PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f;
        float damage       = (autoDamage + AutoDamageBonus + passiveBonus) * comboMult * passiveMult;

        Transform playerTr = GetComponentInParent<PlayerMovement>()?.transform ?? transform;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        GameObject go = Instantiate(daggerPrefab, playerTr.position, Quaternion.AngleAxis(angle, Vector3.forward));
        Dagger     d  = go.GetComponent<Dagger>();
        if (d == null) return;

        d.damage          = damage;
        d.speed           = autoSpeed;
        d.lifetime        = autoRange / autoSpeed;
        d.weaponType      = weaponType;
        d.dotApplications = new List<DotType>(dotApplications);
        d.direction       = aimDirection;
    }

    void CancelCharge()
    {
        isCharging      = false;
        daggerCount     = 0;
        pressMultiplier = 1f;
        BeatConductor.OnBeat -= OnBeat;
    }
}
