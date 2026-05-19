using UnityEngine;
using System.Collections.Generic;

public class OrbitalObject : MonoBehaviour
{
    public float damage;

    private Transform _playerTransform;
    private float _orbitRadius;
    private float _hitCooldown;
    private float _orbitRotationSpeed;
    private float _selfRotationSpeed;
    private EnemyType _weaponType;
    private List<DotType> _dotApplications;

    private float _currentAngle;
    private float _targetAngle;
    private float _selfRotation;

    private readonly Dictionary<int, float> _hitTimestamps = new();

    public void Initialize(Transform playerTransform, float orbitRadius, float dmg,
                           float hitCooldown, float orbitRotationSpeed, float selfRotationSpeed,
                           EnemyType weaponType, List<DotType> dots)
    {
        _playerTransform    = playerTransform;
        _orbitRadius        = orbitRadius;
        damage              = dmg;
        _hitCooldown        = hitCooldown;
        _orbitRotationSpeed = orbitRotationSpeed;
        _selfRotationSpeed  = selfRotationSpeed;
        _weaponType         = weaponType;
        _dotApplications    = dots;
    }

    public void RotateBy(float degrees)
    {
        _targetAngle += degrees;
    }

    void Update()
    {
        if (_playerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, _targetAngle, _orbitRotationSpeed * Time.deltaTime);

        float rad = _currentAngle * Mathf.Deg2Rad;
        transform.position = _playerTransform.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * _orbitRadius;

        _selfRotation += _selfRotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, 0f, _selfRotation);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        int id = collision.gameObject.GetInstanceID();
        if (_hitTimestamps.TryGetValue(id, out float last) && Time.time - last < _hitCooldown) return;
        _hitTimestamps[id] = Time.time;

        EnemyHealth eh = collision.GetComponent<EnemyHealth>();
        if (eh == null) return;

        float mult = (ComboSystem.Instance    != null ? ComboSystem.Instance.GetCurrentMultiplier()    : 1f)
                   * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier()  : 1f);
        eh.TakeDamage(damage * mult, _weaponType);

        if (_dotApplications != null && _dotApplications.Count > 0)
        {
            DotReceiver dr = collision.GetComponent<DotReceiver>();
            if (dr != null) foreach (var dot in _dotApplications) dr.Apply(dot);
        }
    }
}
