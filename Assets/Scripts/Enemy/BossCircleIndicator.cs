using UnityEngine;
using System.Collections;

// Spawned by BossCircleAOEAttack. Plays the forecast animation synced to the beat,
// deals damage, then transitions directly into the explosion animation on the same object.
public class BossCircleIndicator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] AnimationClip forecastClip;
    [SerializeField] AnimationClip explosionClip;
    [SerializeField] string explodeTrigger = "Explode";

    float damage;
    float radius;

    public void Init(float damage, float radius)
    {
        this.damage = damage;
        this.radius = radius;
        transform.localScale = Vector3.one * radius * 2f;
    }

    public void Begin(float forecastBeats, float spb)
    {
        StartCoroutine(Run(forecastBeats * spb));
    }

    IEnumerator Run(float forecastTime)
    {
        if (animator != null && forecastClip != null && forecastClip.length > 0f)
            animator.speed = forecastClip.length / forecastTime;

        yield return new WaitForSeconds(forecastTime);

        // Deal damage at the exact moment the explosion starts
        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var col in hits)
            if (col.CompareTag("Player"))
                col.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        // Snap to explosion animation at natural speed
        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetTrigger(explodeTrigger);
        }

        float explosionDuration = explosionClip != null ? explosionClip.length : 0.4f;
        yield return new WaitForSeconds(explosionDuration);

        Destroy(gameObject);
    }
}
