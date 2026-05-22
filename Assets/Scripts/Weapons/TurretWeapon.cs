using UnityEngine;
using System.Collections.Generic;

public class TurretWeapon : Weapon
{
    public GameObject turretPrefab;
    public GameObject projectilePrefab;

    [Header("Turret Settings")]
    public int   maxTurrets      = 3;
    public int   chargeBeats     = 2;
    public int   lifetimeBeats   = 8;
    public float turretMaxHealth = 100f;
    public float searchRadius    = 20f;

    [Header("Damage")]
    public float baseDamage    = 10f;
    public float goodBonus     = 5f;
    public float perfectBonus  = 10f;
    public float autoDamage    = 6f;

    private readonly List<TurretObject> _turrets = new();
    private TurretObject _chargingTurret;
    private bool   _isHolding;
    private bool   _survivedBeat;
    private string _storedJudgement;
    private float  _storedPendingBonus;

    public override bool IsAttacking => _isHolding;

    public override float GetAutoDamage() => autoDamage + AutoDamageBonus;
    public override float GetDamageForJudgement(string j) =>
        j == "Perfect" ? baseDamage + perfectBonus + PerfectDamageBonus + ActiveDamageBonus :
        j == "Good"    ? baseDamage + goodBonus    + GoodDamageBonus    + ActiveDamageBonus :
        j == "Auto"    ? autoDamage + AutoDamageBonus : 0f;

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Auto" || judgement == "Bad" || _isHolding) return;

        _isHolding               = true;
        _survivedBeat            = false;
        _storedJudgement         = judgement;
        _storedPendingBonus      = PassiveManager.Instance != null ? PassiveManager.Instance.PendingAttackBonus : 0f;
        PlayerMovement.IsLocked  = true;

        BeatConductor.OnBeat += OnBeat;
    }

    void Update()
    {
        if (!_isHolding) return;
        if (!Input.GetKey(KeyCode.P) && !Input.GetMouseButton(0))
            OnRelease();
    }

    void OnBeat()
    {
        if (Time.timeScale == 0) return;
        _survivedBeat = true;

        if (_chargingTurret == null)
            SpawnChargingTurret();

        _chargingTurret.ChargeTick();

        if (_chargingTurret.IsFullyCharged)
        {
            FinalizeAndAddTurret();
            BeatConductor.OnBeat -= OnBeat;
            _isHolding               = false;
            PlayerMovement.IsLocked  = false;
        }
    }

    void OnRelease()
    {
        BeatConductor.OnBeat -= OnBeat;
        _isHolding              = false;
        PlayerMovement.IsLocked = false;

        if (!_survivedBeat)
        {
            // Quick tap — boost all live turrets' next shot
            float bonus = TapBonus() + _storedPendingBonus;
            foreach (var t in _turrets)
                if (t != null) t.SetBonus(bonus);
        }
        else if (_chargingTurret != null)
        {
            // Released before max charge — deploy at current health
            FinalizeAndAddTurret();
        }
        // else: already finalized at max charge inside OnBeat
    }

    void SpawnChargingTurret()
    {
        Transform playerTr = GetComponentInParent<PlayerMovement>()?.transform ?? transform;
        GameObject go = Instantiate(turretPrefab, playerTr.position, Quaternion.identity);
        _chargingTurret = go.GetComponent<TurretObject>();
        _chargingTurret.InitCharge(chargeBeats, turretMaxHealth);
    }

    void FinalizeAndAddTurret()
    {
        float deployDamage = _storedJudgement == "Perfect"
            ? baseDamage + perfectBonus + PerfectDamageBonus + ActiveDamageBonus
            : baseDamage + goodBonus    + GoodDamageBonus    + ActiveDamageBonus;

        _chargingTurret.Deploy(deployDamage, lifetimeBeats, projectilePrefab,
                               searchRadius, weaponType,
                               new List<DotType>(dotApplications), this);
        _turrets.Add(_chargingTurret);
        _chargingTurret = null;

        EnforceCap();
    }

    void EnforceCap()
    {
        while (_turrets.Count > maxTurrets)
        {
            var oldest = _turrets[0];
            _turrets.RemoveAt(0);
            oldest?.ForceDestroy();
        }
    }

    float TapBonus() =>
        _storedJudgement == "Perfect" ? perfectBonus + PerfectDamageBonus
                                      : goodBonus    + GoodDamageBonus;

    public void RemoveTurret(TurretObject t) => _turrets.Remove(t);

    void OnDisable()
    {
        if (_isHolding)
        {
            BeatConductor.OnBeat -= OnBeat;
            _isHolding = false;
            PlayerMovement.IsLocked = false;
            if (_chargingTurret != null)
            {
                Destroy(_chargingTurret.gameObject);
                _chargingTurret = null;
            }
        }
    }
}
