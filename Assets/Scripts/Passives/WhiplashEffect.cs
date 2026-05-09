using UnityEngine;

public class WhiplashEffect : PassiveEffect
{
    public static WhiplashEffect Instance { get; private set; }
    public static bool IsActive => Instance != null && Instance.enabled;

    [Header("Infinite Scaling")]
    public int   hitsPerStack       = 15;
    public float multiplierPerStack = 0.5f;

    int   maxTierMinCombo;
    float maxTierMultiplier;
    int   lastStacks = -1;
    bool  wasAboveMaxTier;

    public override void OnActivate()
    {
        Instance = this;

        LevelSystem levelSystem = FindObjectOfType<LevelSystem>();
        if (levelSystem != null) levelSystem.xpBlocked = true;

        CacheMaxTier();

        ComboSystem.OnComboChanged += OnComboChanged;
        ComboSystem.OnComboReset   += OnComboReset;

        NotifyActiveState(false);
        NotifyStackCount("");
    }

    void OnDisable()
    {
        ComboSystem.OnComboChanged -= OnComboChanged;
        ComboSystem.OnComboReset   -= OnComboReset;

        LevelSystem levelSystem = FindObjectOfType<LevelSystem>();
        if (levelSystem != null) levelSystem.xpBlocked = false;
    }

    void CacheMaxTier()
    {
        var tiers = ComboSystem.Instance?.tiers;
        if (tiers == null || tiers.Length == 0) return;

        ComboTier maxTier = tiers[0];
        foreach (var t in tiers)
            if (t.minCombo > maxTier.minCombo) maxTier = t;

        maxTierMinCombo    = maxTier.minCombo;
        maxTierMultiplier  = maxTier.damageMultiplier;
    }

    public float GetExtraMultiplier(int comboCount)
    {
        if (comboCount <= maxTierMinCombo) return 0f;
        int stacks = (comboCount - maxTierMinCombo) / hitsPerStack;
        return stacks * multiplierPerStack;
    }

    void OnComboChanged(int combo)
    {
        bool isAboveMaxTier = combo > maxTierMinCombo;

        if (isAboveMaxTier != wasAboveMaxTier)
        {
            wasAboveMaxTier = isAboveMaxTier;
            NotifyActiveState(isAboveMaxTier);
        }

        if (!isAboveMaxTier)
        {
            if (lastStacks != 0)
            {
                lastStacks = 0;
                NotifyStackCount("");
            }
            return;
        }

        int newStacks = (combo - maxTierMinCombo) / hitsPerStack;
        if (newStacks != lastStacks)
        {
            lastStacks = newStacks;
            float totalMultiplier = maxTierMultiplier + newStacks * multiplierPerStack;
            NotifyStackCount($"x{totalMultiplier:F1}");
        }
    }

    void OnComboReset()
    {
        wasAboveMaxTier = false;
        lastStacks      = -1;
        NotifyActiveState(false);
        NotifyStackCount("");
    }
}
