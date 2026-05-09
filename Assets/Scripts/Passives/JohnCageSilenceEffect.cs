using UnityEngine;

public class JohnCageSilenceEffect : PassiveEffect
{
    public static JohnCageSilenceEffect Instance { get; private set; }
    public static bool IsActive => Instance != null && Instance.enabled;

    float stackedDamage;

    public override void OnActivate()
    {
        Instance = this;
        InputJudge.OnJudgement += OnJudgement;
        NotifyActiveState(false);
        NotifyStackCount("");
    }

    void OnDisable()
    {
        InputJudge.OnJudgement -= OnJudgement;
    }

    void OnJudgement(string judgement)
    {
        if (judgement != "Perfect" && judgement != "Good") return;
        if (stackedDamage <= 0f) return;

        if (PassiveManager.Instance != null)
            PassiveManager.Instance.PendingAttackBonus = stackedDamage;

        stackedDamage = 0f;
        NotifyActiveState(false);
        NotifyStackCount("");
    }

    public void AccumulateDamage(float amount)
    {
        if (Time.timeScale == 0f) return;
        stackedDamage += amount;
        NotifyActiveState(true);
        NotifyStackCount(Mathf.RoundToInt(stackedDamage).ToString());
    }
}
