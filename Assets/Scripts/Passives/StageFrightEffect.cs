using UnityEngine;

public class StageFrightEffect : PassiveEffect
{
    [Header("Ranges")]
    public float outerRange = 3f;
    public float innerRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Multipliers")]
    public float outerMultiplier = 1.5f;
    public float innerMultiplier = 2f;

    int currentTier = 0; // 0 = none, 1 = outer, 2 = inner

    public override void OnActivate()
    {
        NotifyActiveState(false);
        NotifyStackCount("");
    }

    void Update()
    {
        float nearest = GetNearestEnemyDistance();

        int newTier = 0;
        if      (nearest <= innerRange) newTier = 2;
        else if (nearest <= outerRange) newTier = 1;

        if (newTier == currentTier) return;

        currentTier = newTier;
        NotifyActiveState(currentTier > 0);

        switch (currentTier)
        {
            case 0: NotifyStackCount("");     break;
            case 1: NotifyStackCount("x1.5"); break;
            case 2: NotifyStackCount("x2");   break;
        }
    }

    float GetNearestEnemyDistance()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, outerRange, enemyLayer);
        float nearest = float.MaxValue;
        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < nearest) nearest = dist;
        }
        return nearest;
    }

    public override float GetDamageMultiplier()
    {
        if (currentTier == 2) return innerMultiplier;
        if (currentTier == 1) return outerMultiplier;
        return 1f;
    }
}
