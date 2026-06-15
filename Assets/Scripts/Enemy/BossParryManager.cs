using UnityEngine;

// Manages parry windows for boss attacks 5 and 6.
// Place on any always-active scene GameObject (e.g. GameManager).
public class BossParryManager : MonoBehaviour
{
    public static BossParryManager Instance { get; private set; }

    public static event System.Action OnParrySuccess;
    public static event System.Action OnParryFail;

    [SerializeField] GameObject parryForecastPrefab;
    [SerializeField] GameObject parrySuccessPrefab;
    [SerializeField] float forecastSpawnOffsetY = 1f;

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

    // Called at the moment a parriable projectile is fired.
    // bossHealth.transform is used as the forecast spawn position.
    // windowBeats should equal the projectile's travel time so the window
    // expires exactly when the projectile reaches the player.
    public void OpenWindow(BossProjectile projectile, float windowBeats, EnemyHealth bossHealth, float parryDamage)
    {
        activeProjectile = projectile;
        activeBossHealth = bossHealth;
        activeParryDamage = parryDamage;
        float spb = BeatConductor.Instance != null ? BeatConductor.Instance.secondsPerBeat : 0.5f;
        windowEndTime = Time.unscaledTime + windowBeats * spb;
        windowOpen = true;

        // Forecast spawns on the boss — plays to completion regardless of outcome.
        // This gives the player exactly windowBeats to react after seeing the flash.
        if (parryForecastPrefab != null && bossHealth != null)
        {
            Vector3 spawnPos = bossHealth.transform.position + Vector3.up * forecastSpawnOffsetY;
            Instantiate(parryForecastPrefab, spawnPos, Quaternion.identity);
        }

        if (BeatMarkerLane.Instance != null && BeatConductor.Instance != null)
            BeatMarkerLane.Instance.SpawnParryMarker(BeatConductor.Instance.songPosition + windowBeats * spb);
        // TODO: audio cue — "parry incoming" sound
    }

    void OnParryInput()
    {
        if (!windowOpen) return;

        if (Time.unscaledTime <= windowEndTime && activeProjectile != null)
        {
            if (parrySuccessPrefab != null)
                Instantiate(parrySuccessPrefab, activeProjectile.transform.position, Quaternion.identity);
            Destroy(activeProjectile.gameObject);
            activeBossHealth?.TakeDamage(activeParryDamage);
            OnParrySuccess?.Invoke();
            // TODO: audio cue — parry success sound
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
