using UnityEngine;

public class DissonanceEffect : PassiveEffect
{
    [Header("Bonus Settings")]
    public float bonusMultiplier   = 1.5f;
    public int   buffDurationBeats = 5;

    int  beatsRemaining;
    bool isActive;

    public override void OnActivate()
    {
        PlayerHealth.OnDamageTaken += OnDamageTaken;
        BeatConductor.OnBeat       += OnBeat;

        NotifyActiveState(false);
        NotifyStackCount("");
    }

    void OnDisable()
    {
        PlayerHealth.OnDamageTaken -= OnDamageTaken;
        BeatConductor.OnBeat       -= OnBeat;
    }

    void OnDamageTaken()
    {
        beatsRemaining = buffDurationBeats;

        if (!isActive)
        {
            isActive = true;
            NotifyActiveState(true);
            NotifyStackCount($"x{bonusMultiplier:F1}");
        }
    }

    void OnBeat()
    {
        if (Time.timeScale == 0f) return;
        if (!isActive) return;

        beatsRemaining--;
        if (beatsRemaining > 0) return;

        isActive = false;
        NotifyActiveState(false);
        NotifyStackCount("");
    }

    public override float GetDamageMultiplier() => isActive ? bonusMultiplier : 1f;
}
