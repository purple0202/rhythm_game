using UnityEngine;
using System.Collections;

public abstract class BossAttack : MonoBehaviour
{
    public enum StartGrid { QuarterBeat, HalfBeat, FullBeat }

    [Header("Base")]
    [SerializeField] float cooldownBeats = 2f;
    public StartGrid startGrid = StartGrid.QuarterBeat;

    protected Transform player;
    protected Transform bossTransform;

    bool isRunning;
    float cooldownTimer;
    bool localIsPhase2;

    public bool IsRunning => isRunning;
    public bool IsOnCooldown => cooldownTimer > 0f;
    public virtual bool IsParryable => false;
    protected bool IsPhase2 => localIsPhase2;

    protected virtual void Awake() => bossTransform = transform;

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public bool CanActivate(bool isPhase2) =>
        !isRunning && !IsOnCooldown && CanActivateInternal(isPhase2);

    protected virtual bool CanActivateInternal(bool isPhase2) => true;

    public void Activate(Transform playerTransform, bool isPhase2)
    {
        player = playerTransform;
        localIsPhase2 = isPhase2;
        isRunning = true;
        StartCoroutine(RunAttack());
    }

    IEnumerator RunAttack()
    {
        yield return ExecuteAttack();
        isRunning = false;
        cooldownTimer = cooldownBeats * Spb();
    }

    protected abstract IEnumerator ExecuteAttack();

    // WaitForSeconds (scaled) so attacks pause correctly during upgrade screens
    protected WaitForSeconds WaitBeats(float beats) =>
        new WaitForSeconds(beats * Spb());

    protected float Spb() =>
        BeatConductor.Instance != null ? BeatConductor.Instance.secondsPerBeat : 0.5f;
}
