using UnityEngine;

public class WeaponBox : MonoBehaviour
{
    [Tooltip("The weapon prefab to grant the player on pickup.")]
    // public Weapon weaponPrefab;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        WeaponController controller = other.GetComponent<WeaponController>();
        if (controller == null) return;

        // Weapon weaponInstance = Instantiate(weaponPrefab, other.transform);
        controller.EquipWeapon();

        Destroy(gameObject);
    }
}
