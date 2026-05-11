using UnityEngine;
using System.Collections;

public class LightningWeapon : Weapon
{
    [Header("Damage")]
    public float perfectDamage = 20f;
    public float goodDamage = 12f;

    [Header("Strike Settings")]
    public float strikeRadius = 1.5f;
    public float perfectRadiusBonus = 0.5f;
    public LayerMask enemyLayer;

    [Header("Effect")]
    public GameObject strikeEffectPrefab;
    public float effectDuration = 0.4f;

    public float autoDamage = 8f;

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? perfectDamage + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? goodDamage    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage    + AutoDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        float damage;
        float radius;

        float sizeMult = 1f + SizeBonus * 0.1f;
        if (judgement == "Perfect")
        {
            damage = perfectDamage + PerfectDamageBonus + ActiveDamageBonus;
            radius = (strikeRadius + perfectRadiusBonus) * sizeMult;
        }
        else if (judgement == "Good")
        {
            damage = goodDamage + GoodDamageBonus + ActiveDamageBonus;
            radius = strikeRadius * sizeMult;
        }
        else if (judgement == "Auto")
        {
            damage = autoDamage + AutoDamageBonus;
            radius = strikeRadius * sizeMult;
        }
        else return; // "Bad"

        Vector3 strikePos = GetRandomScreenPosition();
        StartCoroutine(Strike(strikePos, damage, radius));
    }

    Vector3 GetRandomScreenPosition()
    {
        Vector3 viewport = new Vector3(
            Random.Range(0.05f, 0.95f),
            Random.Range(0.05f, 0.95f),
            Camera.main.nearClipPlane
        );
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewport);
        worldPos.z = 0f;
        return worldPos;
    }

    IEnumerator Strike(Vector3 position, float damage, float radius)
    {
        if (strikeEffectPrefab != null)
        {
            GameObject effect = Instantiate(strikeEffectPrefab, position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        // Small delay so the visual lands before damage resolves
        yield return new WaitForSeconds(0.05f);

        float multiplier = (ComboSystem.Instance != null ? ComboSystem.Instance.GetCurrentMultiplier() : 1f)
                         * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f);
        float bonus = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage((damage + bonus) * multiplier, weaponType);
                ApplyDots(hit.gameObject);
            }
        }
    }
}
