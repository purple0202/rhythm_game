using UnityEngine;

// Spawned by BossCircleIndicator when its forecast completes.
// Handles the actual AOE damage — the indicator itself is just visual.
public class BossExplosion : MonoBehaviour
{
    [SerializeField] float lifetime = 0.4f;

    public void Init(float damage, float radius)
    {
        transform.localScale = Vector3.one * radius * 2f;

        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
                col.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }

        // TODO: explosion VFX / sound here
        Destroy(gameObject, lifetime);
    }
}
