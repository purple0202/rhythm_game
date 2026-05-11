using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats (never changed at runtime)")]
    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 5f;

    [Header("Flat Upgrade Bonuses")]
    public float bonusAutoDamage        = 0f;   // Metronome
    public float bonusActiveDamage      = 0f;   // Fine Tuning
    public float bonusPerfectDamage     = 0f;   // The Baton
    public float bonusGoodDamage        = 0f;   // Bassline
    public float bonusMoveSpeed         = 0f;   // Uptempo
    public float bonusMaxHealth         = 0f;
    public float weaponSizeBonus        = 0f;   // Larger Amp
    public float damageReductionPercent = 0f;   // Sound Engineer (0–100)
    public int   projectileBonus        = 0;    // Double Time
    public float dashCooldownReduction  = 0f;   // Roadie
    public float enemySpawnMultiplier   = 1f;   // Popularity

    public float TotalMoveSpeed => baseMoveSpeed + bonusMoveSpeed;
    public float TotalMaxHealth => baseMaxHealth + bonusMaxHealth;

    void Awake() => Instance = this;

    public bool HasWeapon(string weaponName) => true;
}
