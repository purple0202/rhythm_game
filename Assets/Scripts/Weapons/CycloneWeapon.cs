using UnityEngine;
using System.Collections.Generic;

public class CycloneWeapon : Weapon
{
    [Header("Damage")]
    public float damage = 10f;

    [Header("Lifetime")]
    public int lifetimeBeats = 8;

    [Header("Growth")]
    public int maxGrowthTaps = 3;
    public float baseScale = 1f;
    [Tooltip("Multiplicative size change on Bad hit (e.g. -0.3 = 30% smaller)")]
    public float badScaleChange = -0.3f;
    [Tooltip("Multiplicative size change on Good hit (e.g. 0.3 = 30% larger)")]
    public float goodScaleChange = 0.3f;
    [Tooltip("Multiplicative size change on Perfect hit (e.g. 0.5 = 50% larger)")]
    public float perfectScaleChange = 0.5f;

    [Header("Cap")]
    public int maxCyclones = 3;

    [Header("Prefab")]
    public GameObject cyclonePrefab;

    private readonly List<Cyclone> activeCyclones = new();
    private Cyclone currentCyclone;
    private int growthTapsApplied;

    public override float GetAutoDamage() => 0f;
    public override float GetDamageForJudgement(string j) =>
        j != "Bad" ? damage + ActiveDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Auto") return;
        if (cyclonePrefab == null) return;

        float scaleChange = judgement == "Perfect" ? perfectScaleChange :
                            judgement == "Good"    ? goodScaleChange :
                            judgement == "Bad"     ? badScaleChange : 0f;

        CleanDead();

        bool spawnNew = currentCyclone == null || growthTapsApplied >= maxGrowthTaps;

        if (spawnNew)
            SpawnCyclone(scaleChange);
        else
            GrowCurrent(scaleChange);
    }

    private void SpawnCyclone(float scaleChange)
    {
        if (activeCyclones.Count >= maxCyclones)
        {
            Cyclone oldest = activeCyclones[0];
            activeCyclones.RemoveAt(0);
            if (oldest != null) Destroy(oldest.gameObject);
        }

        Vector3 spawnPos = GetComponentInParent<PlayerMovement>()?.transform.position ?? transform.position;
        GameObject go = Instantiate(cyclonePrefab, spawnPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * baseScale;

        Cyclone c = go.GetComponent<Cyclone>();
        if (c != null)
        {
            c.damage = damage + ActiveDamageBonus
                     + (PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f);
            c.lifetimeBeats = lifetimeBeats;
            c.weaponType = weaponType;
            c.dotApplications = new List<DotType>(dotApplications);
        }

        ApplyScaleChange(go, scaleChange);

        activeCyclones.Add(c);
        currentCyclone = c;
        growthTapsApplied = 0;
    }

    private void GrowCurrent(float scaleChange)
    {
        ApplyScaleChange(currentCyclone.gameObject, scaleChange);
        growthTapsApplied++;
    }

    private void ApplyScaleChange(GameObject go, float scaleChange)
    {
        go.transform.localScale *= (1f + scaleChange);
    }

    private void CleanDead()
    {
        activeCyclones.RemoveAll(c => c == null);

        // If currentCyclone was destroyed, reset tap counter
        if (currentCyclone == null)
            growthTapsApplied = 0;
    }
}
