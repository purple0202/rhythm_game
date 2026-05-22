using UnityEngine;
using System.Collections;

public class LaserBeamWeapon : Weapon
{
    enum State { Idle, Charging, Firing }

    [Header("Charge Damage")]
    public float baseDamage = 5f;
    [Tooltip("Bonus tick damage per Perfect input during the 4-beat charge (max +4)")]
    public float damagePerPerfect = 8f;

    [Header("Firing Press Bonuses")]
    [Tooltip("Added to tick damage when player presses Perfect while firing")]
    public float perfectPressBonus = 15f;
    [Tooltip("Added to tick damage when player presses Good while firing")]
    public float goodPressBonus = 7f;
    [Tooltip("Added to tick damage when player presses Bad while firing")]
    public float badPressBonus = 0f;

    [Header("Laser")]
    public float laserWidth = 0.8f;
    public GameObject laserPrefab;

    [Header("Detection")]
    public LayerMask enemyLayer;

    private State state = State.Idle;
    private int chargeCount;
    private int perfectCount;
    private float chargedDamage;
    private float currentTickDamage;
    private Vector2 lastDirection = Vector2.right;
    private Vector2 firingDirection;
    private GameObject laserGO;

    void OnEnable()
    {
        PlayerDash.OnDashStarted += OnDashStarted;
        ResetCharge();
    }

    void OnDisable()
    {
        PlayerDash.OnDashStarted -= OnDashStarted;
        StopFiring();
        ResetCharge();
    }

    void OnDestroy()
    {
        // Safety unsubscribe in case the GO is destroyed while active
        PlayerDash.OnDashStarted -= OnDashStarted;
        BeatConductor.OnBeat -= OnFiringBeat;
    }

    void Update()
    {
        // Track 4-directional aim; last valid input persists as fallback
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(x) > 0.1f || Mathf.Abs(y) > 0.1f)
        {
            if (Mathf.Abs(x) >= Mathf.Abs(y))
                lastDirection = x > 0 ? Vector2.right : Vector2.left;
            else
                lastDirection = y > 0 ? Vector2.up : Vector2.down;
        }

        if (state == State.Firing)
            UpdateLaserVisual();
    }

    public override float GetAutoDamage() => 0f;
    public override float GetDamageForJudgement(string j) =>
        j != "Auto" ? baseDamage + 4f * damagePerPerfect + ActiveDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Auto") return;

        if (state == State.Firing)
        {
            AdjustTickDamage(judgement);
            return;
        }

        // Inputs 1–4: charge. Input 5: fire.
        state = State.Charging;
        if (chargeCount < 4)
        {
            if (judgement == "Perfect") perfectCount++;
            chargeCount++;
        }
        else
        {
            FireLaser();
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void FireLaser()
    {
        chargedDamage = baseDamage + perfectCount * damagePerPerfect + ActiveDamageBonus
                      + (PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f);
        currentTickDamage = chargedDamage;
        firingDirection = lastDirection;
        state = State.Firing;

        if (laserPrefab != null)
        {
            laserGO = Instantiate(laserPrefab);
            UpdateLaserVisual();
        }

        BeatConductor.OnBeat += OnFiringBeat;
    }

    private void StopFiring()
    {
        if (state != State.Firing) return;
        BeatConductor.OnBeat -= OnFiringBeat;

        if (laserGO != null) { Destroy(laserGO); laserGO = null; }

        ResetCharge();
    }

    private void ResetCharge()
    {
        chargeCount = 0;
        perfectCount = 0;
        state = State.Idle;
    }

    private void AdjustTickDamage(string judgement)
    {
        float bonus = judgement == "Perfect" ? perfectPressBonus :
                      judgement == "Good"    ? goodPressBonus :
                      badPressBonus;
        currentTickDamage = chargedDamage + bonus;
    }

    private void OnDashStarted(Vector2 startPos, Vector2 endPos) => StopFiring();

    private void OnFiringBeat()
    {
        if (Time.timeScale == 0 || state != State.Firing) return;
        DealLaserDamage();
        StartCoroutine(HalfBeatDamage());
    }

    private IEnumerator HalfBeatDamage()
    {
        yield return new WaitForSecondsRealtime(BeatConductor.Instance.secondsPerBeat * 0.5f);
        if (state == State.Firing) DealLaserDamage();
    }

    private void DealLaserDamage()
    {
        Vector3 playerPos = GetComponentInParent<PlayerMovement>()?.transform.position ?? transform.position;
        float length = GetLaserLength(firingDirection, playerPos);
        float width = laserWidth * (1f + SizeBonus * 0.1f);

        Vector2 boxCenter = (Vector2)playerPos + firingDirection * length * 0.5f;
        Vector2 boxSize = firingDirection.x != 0
            ? new Vector2(length, width)
            : new Vector2(width, length);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, enemyLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            EnemyHealth health = hit.GetComponent<EnemyHealth>();
            if (health == null) continue;

            float mult = (ComboSystem.Instance != null ? ComboSystem.Instance.GetCurrentMultiplier() : 1f)
                       * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f);
            health.TakeDamage(currentTickDamage * mult, weaponType);
            ApplyDots(hit.gameObject);
        }
    }

    private void UpdateLaserVisual()
    {
        if (laserGO == null) return;
        Vector3 playerPos = GetComponentInParent<PlayerMovement>()?.transform.position ?? transform.position;
        float length = GetLaserLength(firingDirection, playerPos);
        float width = laserWidth * (1f + SizeBonus * 0.1f);

        laserGO.transform.position = playerPos + (Vector3)(firingDirection * length * 0.5f);
        laserGO.transform.localScale = firingDirection.x != 0
            ? new Vector3(length, width, 1f)
            : new Vector3(width, length, 1f);
    }

    private float GetLaserLength(Vector2 dir, Vector3 playerPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return 20f;

        if (dir.x > 0)
            return cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, cam.nearClipPlane)).x - playerPos.x;
        if (dir.x < 0)
            return playerPos.x - cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, cam.nearClipPlane)).x;
        if (dir.y > 0)
            return cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, cam.nearClipPlane)).y - playerPos.y;
        return playerPos.y - cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, cam.nearClipPlane)).y;
    }
}
