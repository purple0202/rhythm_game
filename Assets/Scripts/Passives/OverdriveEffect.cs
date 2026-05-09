using UnityEngine;

public class OverdriveEffect : PassiveEffect
{
    [Header("Pulse Settings")]
    public float pulseRadius = 3f;
    public LayerMask enemyLayer;

    [Header("Visual")]
    public GameObject pulseEffectPrefab; // assign when art is ready

    int   maxTierMinCombo;
    int   attacksAtMaxTier;
    bool  isAtMaxTier;
    float lastDamage;
    EnemyType lastWeaponType;

    public override void OnActivate()
    {
        CacheMaxTier();
        WeaponController.OnAttackFired  += OnAttackFired;
        ComboSystem.OnComboChanged      += OnComboChanged;
        ComboSystem.OnComboReset        += OnComboReset;
        NotifyActiveState(false);
    }

    void OnDisable()
    {
        WeaponController.OnAttackFired  -= OnAttackFired;
        ComboSystem.OnComboChanged      -= OnComboChanged;
        ComboSystem.OnComboReset        -= OnComboReset;
    }

    void CacheMaxTier()
    {
        var tiers = ComboSystem.Instance?.tiers;
        if (tiers == null || tiers.Length == 0) return;
        int max = 0;
        foreach (var t in tiers)
            if (t.minCombo > max) max = t.minCombo;
        maxTierMinCombo = max;
    }

    void OnComboChanged(int combo)
    {
        bool nowAtMax = combo >= maxTierMinCombo && maxTierMinCombo > 0;
        if (nowAtMax == isAtMaxTier) return;
        isAtMaxTier = nowAtMax;
        if (!isAtMaxTier) attacksAtMaxTier = 0;
        NotifyActiveState(isAtMaxTier);
    }

    void OnComboReset()
    {
        isAtMaxTier      = false;
        attacksAtMaxTier = 0;
        NotifyActiveState(false);
    }

    void OnAttackFired(float damage, EnemyType weaponType)
    {
        lastDamage     = damage;
        lastWeaponType = weaponType;

        if (!isAtMaxTier) return;

        attacksAtMaxTier++;
        if (attacksAtMaxTier % 2 != 0) return;

        FirePulse();
    }

    void FirePulse()
    {
        float multiplier = (ComboSystem.Instance != null ? ComboSystem.Instance.GetCurrentMultiplier() : 1f)
                         * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pulseRadius, enemyLayer);
        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(lastDamage * multiplier, lastWeaponType);
        }

        if (pulseEffectPrefab != null)
            Instantiate(pulseEffectPrefab, transform.position, Quaternion.identity);
    }
}
