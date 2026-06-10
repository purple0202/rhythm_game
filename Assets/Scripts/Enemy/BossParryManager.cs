using UnityEngine;

// Manages parry windows for boss attacks 5 and 6.
// Place on any always-active scene GameObject (e.g. GameManager).
public class BossParryManager : MonoBehaviour
{
    public static BossParryManager Instance { get; private set; }

    public static event System.Action OnParrySuccess;
    public static event System.Action OnParryFail;

    BossProjectile activeProjectile;
    EnemyHealth activeBossHealth;
    float activeParryDamage;
    float windowEndTime;
    bool windowOpen;

    void Awake()
    {
        Instance = this;
        InputJudge.OnParryInput += OnParryInput;
    }

    void OnDestroy() => InputJudge.OnParryInput -= OnParryInput;

    public void OpenWindow(BossProjectile projectile, float windowBeats, EnemyHealth bossHealth, float parryDamage)
    {
        activeProjectile = projectile;
        activeBossHealth = bossHealth;
        activeParryDamage = parryDamage;
        float spb = BeatConductor.Instance != null ? BeatConductor.Instance.secondsPerBeat : 0.5f;
        windowEndTime = Time.unscaledTime + windowBeats * spb;
        windowOpen = true;

        BossHUD.Instance?.ShowParryNotice();
        // TODO: audio cue — "parry incoming" sound
    }

    void OnParryInput()
    {
        if (!windowOpen) return;

        if (Time.unscaledTime <= windowEndTime && activeProjectile != null)
        {
            Destroy(activeProjectile.gameObject);
            activeBossHealth?.TakeDamage(activeParryDamage);
            OnParrySuccess?.Invoke();
            // TODO: parry success VFX + audio
            Debug.Log("PARRY SUCCESS!");
        }

        CloseWindow();
    }

    void Update()
    {
        if (windowOpen && Time.unscaledTime > windowEndTime)
        {
            OnParryFail?.Invoke();
            CloseWindow();
        }
    }

    void CloseWindow()
    {
        windowOpen = false;
        activeProjectile = null;
    }
}
