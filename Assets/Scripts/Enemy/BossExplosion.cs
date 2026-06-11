using UnityEngine;

// Spawned by BossCircleIndicator when its forecast completes.
// Handles the actual AOE damage — the indicator itself is just visual.
public class BossExplosion : MonoBehaviour
{
    [SerializeField] Animator explosionAnimator;
    [SerializeField] float fallbackLifetime = 0.4f;

    public void Init(float damage, float radius)
    {
        transform.localScale = Vector3.one * radius * 2f;

        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
                col.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }

        float lifetime = fallbackLifetime;
        if (explosionAnimator != null)
        {
            var clips = explosionAnimator.runtimeAnimatorController?.animationClips;
            if (clips != null && clips.Length > 0 && clips[0].length > 0f)
                lifetime = clips[0].length;
        }

        Destroy(gameObject, lifetime);
    }
}
