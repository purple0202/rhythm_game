using UnityEngine;
using System.Collections.Generic;

public class TurretObject : MonoBehaviour
{
    [Header("Health Bar")]
    public Transform healthBarFill; // child Transform — set localScale.x to fill percent

    public float currentHealth { get; private set; }
    public float maxHealth     { get; private set; }
    public bool  IsFullyCharged => currentHealth >= maxHealth;
    public bool  IsLive         { get; private set; }

    private float           _healthPerChargeBeat;
    private float           _healthPerLifetimeBeat;
    private float           _baseDamage;
    private float           _pendingBonus;
    private float           _searchRadius;
    private GameObject      _projectilePrefab;
    private EnemyType       _weaponType;
    private List<DotType>   _dotApplications;
    private TurretWeapon    _weapon;

    private float _barInitialScaleX;
    private float _barInitialPosX;

    void Awake()
    {
        if (healthBarFill != null)
        {
            _barInitialScaleX = healthBarFill.localScale.x;
            _barInitialPosX   = healthBarFill.localPosition.x;
        }
    }

    // Called by TurretWeapon immediately after spawning
    public void InitCharge(int chargeBeats, float turretMaxHealth)
    {
        maxHealth             = turretMaxHealth;
        currentHealth         = 0f;
        _healthPerChargeBeat  = maxHealth / chargeBeats;
        UpdateHealthBar();
    }

    // Called by TurretWeapon each beat while holding
    public void ChargeTick()
    {
        currentHealth = Mathf.Min(currentHealth + _healthPerChargeBeat, maxHealth);
        UpdateHealthBar();
    }

    // Called by TurretWeapon when player releases or max charge reached
    public void Deploy(float damage, int lifetimeBeats, GameObject projectilePrefab,
                       float searchRadius, EnemyType weaponType,
                       List<DotType> dots, TurretWeapon weapon)
    {
        _baseDamage            = damage;
        _healthPerLifetimeBeat = maxHealth / lifetimeBeats;
        _projectilePrefab      = projectilePrefab;
        _searchRadius          = searchRadius;
        _weaponType            = weaponType;
        _dotApplications       = dots;
        _weapon                = weapon;
        IsLive        = true;
        currentHealth = maxHealth;
        UpdateHealthBar();

        BeatConductor.OnBeat += OnBeat;
    }

    void OnDestroy()
    {
        BeatConductor.OnBeat -= OnBeat;
    }

    public void SetBonus(float bonus) => _pendingBonus = bonus;

    // For future use by enemies
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();
        if (currentHealth <= 0f) Die();
    }

    public void ForceDestroy() => Die();

    void OnBeat()
    {
        if (Time.timeScale == 0f || !IsLive) return;

        FireAtNearestEnemy();

        currentHealth -= _healthPerLifetimeBeat;
        UpdateHealthBar();

        if (currentHealth <= 0f) Die();
    }

    void FireAtNearestEnemy()
    {
        if (_projectilePrefab == null) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        float bonus = _pendingBonus;
        _pendingBonus = 0f;

        float mult = (ComboSystem.Instance    != null ? ComboSystem.Instance.GetCurrentMultiplier()    : 1f)
                   * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier()  : 1f);

        GameObject go = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
        Projectile p  = go.GetComponent<Projectile>();
        p.damage          = (_baseDamage + bonus) * mult;
        p.weaponType      = _weaponType;
        p.dotApplications = new List<DotType>(_dotApplications);
        p.SetDirection((target.position - transform.position).normalized);
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _searchRadius);
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var h in hits)
        {
            if (!h.CompareTag("Enemy")) continue;
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < minDist) { minDist = d; closest = h.transform; }
        }
        return closest;
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;

        float percent = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

        Vector3 scale = healthBarFill.localScale;
        scale.x = _barInitialScaleX * percent;
        healthBarFill.localScale = scale;

        // Keep bar left-aligned (compensate for center pivot)
        Vector3 pos = healthBarFill.localPosition;
        pos.x = _barInitialPosX - _barInitialScaleX * (1f - percent) * 0.5f;
        healthBarFill.localPosition = pos;
    }

    void Die()
    {
        _weapon?.RemoveTurret(this);
        Destroy(gameObject);
    }
}
