using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MinibossController : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] string bossName = "UNNAMED";
    [SerializeField] DialogueData introDialogue;
    [SerializeField] bool isFinalBoss = false;

    [Header("Debug")]
    [SerializeField] bool bypassReadyChecks = false;

    [Header("Phase 2")]
    [SerializeField, Range(0f, 1f)] float phase2Threshold = 0.5f;
    [SerializeField, Range(0f, 1f)] float parryRushChance = 0.5f;
    [SerializeField] int parryRushAttackCount = 5;
    [SerializeField] float parryRushCooldownBeats = 10f;

    [Header("Spawn Glide")]
    [SerializeField] float preGlideDelay = 3f;
    [SerializeField] float cameraFocusHoldDuration = 6f;
    [SerializeField] float glideHeight = 8f;
    [SerializeField] float glideDuration = 2f;
    [SerializeField] float landingOffsetY = 5f;
    [SerializeField] float landingRandomRangeX = 3f;

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

        StartCoroutine(SpawnGlide());
    }

    IEnumerator SpawnGlide()
    {
        Vector3 landingPos = player.position + new Vector3(Random.Range(-landingRandomRangeX, landingRandomRangeX), landingOffsetY, 0f);
        Vector3 startPos = landingPos + Vector3.up * glideHeight;
        transform.position = startPos;

        if (isFinalBoss)
            BeatConductor.Instance?.SetParameter("Boss states", 1f);
        else
            BeatConductor.Instance?.SetParameter("Mini Boss Spawn", 1f);
        CameraFollow.Instance?.SetFocusOverride(transform);

        // Boss sits visible at the glide start point so the player has time to notice and react.
        yield return new WaitForSeconds(isFinalBoss ? cameraFocusHoldDuration : preGlideDelay);

        float elapsed = 0f;
        while (elapsed < glideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / glideDuration));
            transform.position = Vector3.Lerp(startPos, landingPos, t);
            yield return null;
        }
        transform.position = landingPos;

        CameraFollow.Instance?.ClearFocusOverride();

        if (DialogueUI.Instance != null && introDialogue != null)
        {
            Action onLastLine = isFinalBoss ? () => StartCoroutine(FinalBossDialogueSequence()) : (Action)null;
            DialogueUI.Instance.Play(introDialogue, PlayIntro, onLastLine);
        }
        else
            PlayIntro();
    }

    void PlayIntro()
    {
        if (BossCutsceneUI.Instance != null)
            BossCutsceneUI.Instance.Play(bossName, spriteRenderer?.sprite, OnIntroComplete);
        else
            OnIntroComplete();
    }

    void OnIntroComplete()
    {
        EnemyType type = enemyHealth != null ? enemyHealth.enemyType : EnemyType.None;
        BossHUD.Instance?.Show(bossName, type, 4, enemyHealth);
        attackLoop = StartCoroutine(AttackLoop());
    }

    IEnumerator FinalBossDialogueSequence()
    {
        DialogueUI.Instance.SetInputBlocked(true);
        BeatConductor.Instance?.SetParameter("Boss states", 2f);
        yield return new WaitForSecondsRealtime(11f);
        BeatConductor.Instance?.SetParameter("Boss states", 3f);
        DialogueUI.Instance.AutoClose();
    }

    void OnDeath()
    {
        isAlive = false;
        if (attackLoop != null) StopCoroutine(attackLoop);
        foreach (var a in attacks) a.StopAllCoroutines();
        BossHUD.Instance?.Hide();

        if (isFinalBoss)
            BeatConductor.Instance?.SetParameter("Boss states", 4f);
        else
            BeatConductor.Instance?.SetParameter("mini Kill", 1f);
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
