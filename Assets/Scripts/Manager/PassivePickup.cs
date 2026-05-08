using UnityEngine;

public class PassivePickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (PassiveManager.Instance.IsFull) return;

        PassiveManager.Instance.OpenSelection();
        Destroy(gameObject);
    }
}
