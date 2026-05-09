using System.Collections.Generic;
using UnityEngine;

public class StageDiveEffect : PassiveEffect
{
    [Header("Damage")]
    public float baseDamage = 15f;
    public LayerMask enemyLayer;

    [Header("Hitbox")]
    public float capsuleWidth = 0.5f;

    float defaultDashDistance;

    public override void OnActivate()
    {
        PlayerDash playerDash = GetComponent<PlayerDash>();
        defaultDashDistance = playerDash != null ? playerDash.dashDistance : 3f;

        PlayerDash.OnDashStarted += OnDashStarted;
        NotifyActiveState(true);
    }

    void OnDisable()
    {
        PlayerDash.OnDashStarted -= OnDashStarted;
    }

    void OnDashStarted(Vector2 startPos, Vector2 endPos)
    {
        Vector2 dashVec = endPos - startPos;
        float dashDistance = dashVec.magnitude;
        if (dashDistance <= 0f) return;

        float damage     = baseDamage * (dashDistance / defaultDashDistance);
        float multiplier = PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f;

        Vector2 center      = (startPos + endPos) * 0.5f;
        float   angle       = Vector2.SignedAngle(Vector2.up, dashVec.normalized);
        Vector2 capsuleSize = new Vector2(capsuleWidth, dashDistance + capsuleWidth);

        Collider2D[] hits = Physics2D.OverlapCapsuleAll(center, capsuleSize, CapsuleDirection2D.Vertical, angle, enemyLayer);

        var hitEnemies = new HashSet<EnemyHealth>();
        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null && hitEnemies.Add(enemy))
                enemy.TakeDamage(damage * multiplier, EnemyType.None);
        }
    }
}
