using UnityEngine;

public class WeaponBox : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        WeaponController controller = other.GetComponentInChildren<WeaponController>();
        if (controller == null) return;

        controller.OpenWeaponSelect();
        Destroy(gameObject);
    }
}
