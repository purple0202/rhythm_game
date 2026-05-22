using UnityEngine;
using System.Collections.Generic;

public class GhostCompanion : MonoBehaviour
{
    private Transform _playerTransform;
    private Vector2 _hoverOffset;
    private float _baseDamage;
    private float _followSpeed;
    private float _bobSpeed;
    private float _bobAmount;
    private float _searchRadius;
    private GameObject _projectilePrefab;
    private EnemyType _weaponType;
    private List<DotType> _dotApplications;

    private float _pendingBonus;

    public void Initialize(Transform playerTransform, Vector2 hoverOffset, float baseDamage,
                           float followSpeed, float bobSpeed, float bobAmount, float searchRadius,
                           GameObject projectilePrefab, EnemyType weaponType, List<DotType> dots)
    {
        _playerTransform  = playerTransform;
        _hoverOffset      = hoverOffset;
        _baseDamage       = baseDamage;
        _followSpeed      = followSpeed;
        _bobSpeed         = bobSpeed;
        _bobAmount        = bobAmount;
        _searchRadius     = searchRadius;
        _projectilePrefab = projectilePrefab;
        _weaponType       = weaponType;
        _dotApplications  = dots;

        BeatConductor.OnBeat += OnBeat;
    }

    void OnDestroy()
    {
        BeatConductor.OnBeat -= OnBeat;
    }

    public void SetBonus(float bonus) => _pendingBonus = bonus;

    void Update()
    {
        if (_playerTransform == null) { Destroy(gameObject); return; }

        Vector2 target = (Vector2)_playerTransform.position + _hoverOffset
                       + new Vector2(0f, Mathf.Sin(Time.time * _bobSpeed) * _bobAmount);
        transform.position = Vector2.Lerp(transform.position, target, _followSpeed * Time.deltaTime);
    }

    void OnBeat()
    {
        if (Time.timeScale == 0f || _projectilePrefab == null) return;

        Transform target = FindNearestEnemy();
        if (target == null) { _pendingBonus = 0f; return; }

        float damage = _baseDamage + _pendingBonus;
        _pendingBonus = 0f;

        float mult = (ComboSystem.Instance    != null ? ComboSystem.Instance.GetCurrentMultiplier()    : 1f)
                   * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier()  : 1f);

        GameObject go = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
        Projectile p = go.GetComponent<Projectile>();
        p.damage          = damage * mult;
        p.weaponType      = _weaponType;
        p.dotApplications = new List<DotType>(_dotApplications);
        p.SetDirection((target.position - transform.position).normalized);
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _searchRadius);
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < minDist) { minDist = d; closest = hit.transform; }
        }
        return closest;
    }
}
