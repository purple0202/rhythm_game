using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance { get; private set; }

    [Tooltip("How many beats the player can miss before the combo breaks.")]
    public int forgiveness = 3;

    [Header("Tiers")]
    public ComboTier[] tiers;

    public static event System.Action<int> OnComboChanged;
    public static event System.Action       OnComboReset;

    private int comboCount;
    private int missedBeats;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()  => InputJudge.OnJudgement += HandleJudgement;
    void OnDisable() => InputJudge.OnJudgement -= HandleJudgement;

    void HandleJudgement(string judgement)
    {
        switch (judgement)
        {
            case "Perfect":
            case "Good":
                missedBeats = 0;
                comboCount++;
                OnComboChanged?.Invoke(comboCount);
                break;

            case "Bad":
                BreakCombo();
                break;

            case "Auto":
                if (Time.timeScale == 0f) break;
                missedBeats++;
                if (missedBeats > forgiveness)
                    BreakCombo();
                break;
        }
    }

    void BreakCombo()
    {
        if (comboCount == 0) return;
        comboCount  = 0;
        missedBeats = 0;
        OnComboReset?.Invoke();
        OnComboChanged?.Invoke(0);
    }

    public int GetCombo() => comboCount;

    public ComboTier GetCurrentTier()
    {
        if (tiers == null || tiers.Length == 0) return default;
        ComboTier result = tiers[0];
        foreach (var tier in tiers)
            if (comboCount >= tier.minCombo) result = tier;
        return result;
    }

    public float GetCurrentMultiplier()
    {
        if (tiers == null || tiers.Length == 0) return 1f;
        float multiplier = GetCurrentTier().damageMultiplier;
        if (WhiplashEffect.IsActive)
            multiplier += WhiplashEffect.Instance.GetExtraMultiplier(comboCount);
        return multiplier;
    }
}

[System.Serializable]
public struct ComboTier
{
    public int   minCombo;
    public float damageMultiplier;
    public Color displayColor;
    [Range(0f, 8f)]
    public float hdrIntensity;
    public float emissionRate;
}
