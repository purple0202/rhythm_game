using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats")]
    public float maxHealth = 100;
    public float damage = 10;
    public float moveSpeed = 5;

    void Awake()
    {
        Instance = this;
    }

    public void ApplyLevelUp(int level)
    {
        damage *= 1.1f;
        maxHealth *= 1.1f;
        moveSpeed *= 1.02f;
    }

    public int projectileCount = 1;

    public bool HasWeapon(string weaponName)
    {
        // Replace with your weapon system later
        return true;
    }
}