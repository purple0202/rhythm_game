using UnityEngine;

public class DeadAirEffect : PassiveEffect
{
    [Header("Stacking")]
    public float damagePerBeat = 2f;
    public int maxStacks = 10;          // max bonus = maxStacks * damagePerBeat

    [Header("Decay")]
    [Tooltip("How many seconds of movement it takes to fully drain the bonus")]
    public float decayDuration = 2f;

    [Header("References")]
    public PlayerMovement playerMovement;

    float currentBonus;
    Rigidbody2D rb;

    public override void OnActivate()
    {
        rb = playerMovement != null ? playerMovement.GetComponent<Rigidbody2D>() : null;
        BeatConductor.OnBeat           += OnBeat;
        WeaponController.OnAttackFired += OnAttackFired;
        NotifyActiveState(false);
    }

    void OnDisable()
    {
        BeatConductor.OnBeat           -= OnBeat;
        WeaponController.OnAttackFired -= OnAttackFired;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (rb == null || currentBonus <= 0f) return;

        bool moving = rb.linearVelocity.sqrMagnitude > 0.05f;
        if (!moving) return;

        float maxBonus = maxStacks * damagePerBeat;
        float decayRate = maxBonus / decayDuration;
        currentBonus = Mathf.Max(0f, currentBonus - decayRate * Time.deltaTime);
        UpdateUI();
    }

    void OnBeat()
    {
        if (Time.timeScale == 0f) return;

        bool moving = rb != null && rb.linearVelocity.sqrMagnitude > 0.05f;
        if (moving) return;

        float maxBonus = maxStacks * damagePerBeat;
        if (currentBonus >= maxBonus) return;

        currentBonus = Mathf.Min(currentBonus + damagePerBeat, maxBonus);
        UpdateUI();
    }

    void OnAttackFired(float damage, EnemyType weaponType)
    {
        if (currentBonus <= 0f || PassiveManager.Instance == null) return;
        PassiveManager.Instance.PendingAttackBonus += currentBonus;
    }

    void UpdateUI()
    {
        bool active = currentBonus > 0f;
        NotifyActiveState(active);
        if (active)
            NotifyStackCount($"+{currentBonus:F0}");
    }
}
