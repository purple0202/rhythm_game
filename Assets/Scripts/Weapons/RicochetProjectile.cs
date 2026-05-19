using UnityEngine;
using System.Collections.Generic;

public class RicochetProjectile : MonoBehaviour
{
    private Transform currentTarget;
    private float firstHitDamage;
    private float bounceDamage;
    private float damageMultiplier;
    private int maxBounces;
    private float chainRange;
    private float travelSpeed;
    private EnemyType weaponType;
    private List<DotType> dotApplications;

    private readonly HashSet<GameObject> hitEnemies = new();
    private int bounceCount;
    private bool isFirstHit = true;

    public void Initialize(Transform firstTarget, float fHit, float bHit, float mult,
                           int bounces, float range, float speed,
                           EnemyType wType, List<DotType> dots)
    {
        currentTarget   = firstTarget;
        firstHitDamage  = fHit;
        bounceDamage    = bHit;
        damageMultiplier = mult;
        maxBounces      = bounces;
        chainRange      = range;
        travelSpeed     = speed;
        weaponType      = wType;
        dotApplications = dots;

        FaceTarget();
    }

    void Update()
    {
        if (currentTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            travelSpeed * Time.deltaTime);

        FaceTarget();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;
        if (hitEnemies.Contains(collision.gameObject)) return;
        if (currentTarget != null && collision.gameObject != currentTarget.gameObject) return;

        EnemyHealth eh = collision.GetComponent<EnemyHealth>();
        if (eh == null) return;

        hitEnemies.Add(collision.gameObject);

        float dmg = (isFirstHit ? firstHitDamage : bounceDamage) * damageMultiplier;
        eh.TakeDamage(dmg, weaponType);
        isFirstHit = false;

        if (dotApplications != null && dotApplications.Count > 0)
        {
            DotReceiver dr = collision.GetComponent<DotReceiver>();
            if (dr != null) foreach (var dot in dotApplications) dr.Apply(dot);
        }

        bounceCount++;
        if (bounceCount >= maxBounces)
        {
            Destroy(gameObject);
            return;
        }

        Transform next = FindNextTarget(collision.transform.position);
        if (next == null)
        {
            Destroy(gameObject);
            return;
        }

        currentTarget = next;
    }

    Transform FindNextTarget(Vector2 origin)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, chainRange);
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            if (hitEnemies.Contains(hit.gameObject)) continue;
            float d = Vector2.Distance(origin, hit.transform.position);
            if (d < minDist) { minDist = d; closest = hit.transform; }
        }
        return closest;
    }

    void FaceTarget()
    {
        if (currentTarget == null) return;
        Vector2 dir = (currentTarget.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
