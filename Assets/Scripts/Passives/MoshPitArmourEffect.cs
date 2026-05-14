using UnityEngine;

public class MoshPitArmourEffect : PassiveEffect
{
    [Header("Shield Settings")]
    public float shieldGainAmount = 30f;
    public float decayDuration    = 5f;

    float decayPerSecond;
    PlayerHealth playerHealth;

    public override void OnActivate()
    {
        playerHealth = GetComponent<PlayerHealth>();
        decayPerSecond = shieldGainAmount / decayDuration;
        PlayerDash.OnDashCompleted += OnDashCompleted;
        NotifyActiveState(true);
    }

    void OnDisable()
    {
        PlayerDash.OnDashCompleted -= OnDashCompleted;
    }

    void OnDashCompleted()
    {
        playerHealth.AddShield(shieldGainAmount);
    }

    void Update()
    {
        if (playerHealth == null || playerHealth.currentShield <= 0f) return;
        playerHealth.AddShield(-decayPerSecond * Time.deltaTime);
    }
}
