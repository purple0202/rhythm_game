using UnityEngine;
using System.Collections.Generic;

// Spawned by BossRingPulseAttack. Expands from boss position outward and applies debuffs.
// Visual is a LineRenderer circle — no sprite needed.
public class BossRing : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] Color ringColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] float lineWidth = 0.15f;
    [SerializeField] int segments = 64;

    float damage;
    float ringSpeed;
    float debuffDuration;
    bool isPhase2;
    float currentRadius;
    const float MaxRadius = 35f;

    LineRenderer lr;
    readonly HashSet<int> hitIds = new();

    public void Init(float damage, float ringSpeed, float debuffDuration, bool isPhase2)
    {
        this.damage = damage;
        this.ringSpeed = ringSpeed;
        this.debuffDuration = debuffDuration;
        this.isPhase2 = isPhase2;
        currentRadius = 0.1f;

        BuildLineRenderer();
    }

    void BuildLineRenderer()
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;   // positions are local — scaling transform = scaling circle
        lr.loop = true;
        lr.positionCount = segments;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = ringColor;
        lr.endColor = ringColor;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 5;

        // Unit circle — actual size controlled by transform.localScale
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f));
        }
    }

    void Update()
    {
        currentRadius += ringSpeed * Time.deltaTime;
        transform.localScale = Vector3.one * currentRadius;

        CheckPlayerHit();

        if (currentRadius > MaxRadius)
            Destroy(gameObject);
    }

    void CheckPlayerHit()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, currentRadius);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;

            // Always record the ring as having reached the player so it can't
            // double-hit after a dash ends — the ring has already swept past them.
            if (!hitIds.Add(col.GetInstanceID())) continue;

            // Dashing — ring passes through harmlessly.
            var health = col.GetComponent<PlayerHealth>();
            if (health != null && health.IsInvincible) continue;

            health?.TakeDamage(damage);

            var debuff = PlayerDebuffManager.Instance;
            if (debuff == null) continue;

            debuff.ApplySlow(debuffDuration);
        }
    }
}
