using UnityEngine;

public class CrowdsurfEffect : PassiveEffect
{
    [Header("Detection")]
    public float detectionRadius = 5f;
    public LayerMask enemyLayer;

    [Header("Bonuses")]
    public float damageBonus       = 1.5f;
    public float dashDistanceBonus = 1.5f;
    public float dashDurationBonus = 0.7f; // shorter duration = faster dash

    PlayerDash playerDash;
    float originalDashDistance;
    float originalDashDuration;
    bool isSurrounded;

    public override void OnActivate()
    {
        playerDash = GetComponent<PlayerDash>();
        if (playerDash == null) return;
        originalDashDistance = playerDash.dashDistance;
        originalDashDuration = playerDash.dashDuration;
    }

    void Update()
    {
        bool wasSurrounded = isSurrounded;
        isSurrounded = CheckSurrounded();

        if (isSurrounded != wasSurrounded)
            ApplyDashBonuses(isSurrounded);
    }

    bool CheckSurrounded()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        bool topRight = false, topLeft = false, bottomRight = false, bottomLeft = false;
        foreach (var hit in hits)
        {
            Vector2 dir = (Vector2)(hit.transform.position - transform.position);
            if      (dir.x > 0 && dir.y > 0) topRight    = true;
            else if (dir.x < 0 && dir.y > 0) topLeft     = true;
            else if (dir.x > 0 && dir.y < 0) bottomRight = true;
            else if (dir.x < 0 && dir.y < 0) bottomLeft  = true;
        }
        return topRight && topLeft && bottomRight && bottomLeft;
    }

    void ApplyDashBonuses(bool active)
    {
        if (playerDash == null) return;
        playerDash.dashDistance = active ? originalDashDistance * dashDistanceBonus : originalDashDistance;
        playerDash.dashDuration = active ? originalDashDuration * dashDurationBonus : originalDashDuration;
    }

    public override float GetDamageMultiplier() => isSurrounded ? damageBonus : 1f;

    void OnDisable()
    {
        ApplyDashBonuses(false);
    }
}
