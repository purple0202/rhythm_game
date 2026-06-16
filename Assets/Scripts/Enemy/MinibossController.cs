using UnityEngine;
using System.Collections;
using System.Linq;

public class MinibossController : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] string bossName = "UNNAMED";

    [Header("Debug")]
    [SerializeField] bool bypassReadyChecks = false;

    [Header("Phase 2")]
    [SerializeField, Range(0f, 1f)] float phase2Threshold = 0.5f;
    [SerializeField, Range(0f, 1f)] float parryRushChance = 0.5f;
    [SerializeField] int parryRushAttackCount = 5;
    [SerializeField] float parryRushCooldownBeats = 10f;

    Transform player;
    SpriteRenderer spriteRenderer;
    EnemyHealth enemyHealth;
    BossAttack[] attacks;

    bool isPhase2;
    bool inParryRush;
    int parryRushRemaining;
    float rushCooldownTimer;
    bool isAlive;
    Coroutine attackLoop;

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("MinibossController: no GameObject tagged 'Player' found.");
            OnIntroComplete();
            return;
        }

        player = playerGO.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();
        attacks = GetComponents<BossAttack>().Where(a => a.enabled).ToArray();

        if (enemyHealth != null)
        {
            enemyHealth.onDeath += OnDeath;
            isAlive = true;
        }

        if (BossIntroUI.Instance != null)
            BossIntroUI.Instance.Play(bossName, spriteRenderer?.sprite, OnIntroComplete);
        else
            OnIntroComplete();
    }

    void OnIntroComplete()
    {
        EnemyType type = enemyHealth != null ? enemyHealth.enemyType : EnemyType.None;
        BossHUD.Instance?.Show(bossName, type, 4, enemyHealth);
        attackLoop = StartCoroutine(AttackLoop());
    }

    void OnDeath()
    {
        isAlive = false;
        if (attackLoop != null) StopCoroutine(attackLoop);
        foreach (var a in attacks) a.StopAllCoroutines();
        BossHUD.Instance?.Hide();
    }

    IEnumerator AttackLoop()
    {
        while (isAlive)
        {
            CheckPhaseTransition();

            if (isPhase2 && !inParryRush && rushCooldownTimer <= 0f && Random.value < parryRushChance)
            {
                inParryRush = true;
                parryRushRemaining = parryRushAttackCount;
            }

            BossAttack next = inParryRush ? PickParryAttack() : PickRandomAttack();

            if (next == null)
            {
                // All attacks on cooldown — wait a quarter beat and try again
                yield return new WaitForSeconds(Spb() * 0.25f);
                continue;
            }

            // Snap start time to attack's grid (quarter/half/full beat boundary)
            yield return SnapToGrid(next.startGrid);
            if (!isAlive) yield break;

            next.Activate(player, isPhase2);
            while (next.IsRunning) yield return null;

            if (inParryRush)
            {
                parryRushRemaining--;
                if (parryRushRemaining <= 0)
                {
                    inParryRush = false;
                    rushCooldownTimer = parryRushCooldownBeats * Spb();
                }
            }
        }
    }

    IEnumerator SnapToGrid(BossAttack.StartGrid grid)
    {
        float spb = Spb();
        float now = BeatConductor.Instance?.songPosition ?? 0f;
        float lastBeat = BeatConductor.Instance?.lastBeatTime ?? 0f;
        float timeSinceBeat = now - lastBeat;

        float interval = grid switch
        {
            BossAttack.StartGrid.HalfBeat => spb * 0.5f,
            BossAttack.StartGrid.FullBeat => spb,
            _                             => spb * 0.25f
        };

        float wait = interval - (timeSinceBeat % interval);
        if (wait < 0.02f) wait += interval;  // skip near-zero waits that would feel instant

        yield return new WaitForSeconds(wait);
    }

    BossAttack PickRandomAttack()
    {
        var available = bypassReadyChecks
            ? attacks.Where(a => !a.IsRunning).ToList()
            : attacks.Where(a => a.CanActivate(isPhase2)).ToList();
        return available.Count == 0 ? null : available[Random.Range(0, available.Count)];
    }

    BossAttack PickParryAttack()
    {
        var available = bypassReadyChecks
            ? attacks.Where(a => a.IsParryable && !a.IsRunning).ToList()
            : attacks.Where(a => a.IsParryable && a.CanActivate(isPhase2)).ToList();
        if (available.Count == 0) return PickRandomAttack();
        return available[Random.Range(0, available.Count)];
    }

    void CheckPhaseTransition()
    {
        if (isPhase2 || enemyHealth == null) return;
        if (enemyHealth.CurrentHealth < enemyHealth.maxHealth * phase2Threshold)
        {
            isPhase2 = true;
            Debug.Log("MinibossController: Phase 2!");
            // TODO: phase 2 visual flash + music intensity change
        }
    }

    float Spb() => BeatConductor.Instance != null ? BeatConductor.Instance.secondsPerBeat : 0.5f;

    void Update()
    {
        if (rushCooldownTimer > 0f) rushCooldownTimer -= Time.deltaTime;

        if (player == null || spriteRenderer == null) return;
        spriteRenderer.flipX = player.position.x < transform.position.x;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10);
    }
}
