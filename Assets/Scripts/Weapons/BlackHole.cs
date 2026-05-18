using UnityEngine;

public class BlackHole : MonoBehaviour
{
    [HideInInspector] public float     damagePerBeat;
    [HideInInspector] public int       lifetimeBeats;
    [HideInInspector] public EnemyType weaponType;
    [HideInInspector] public float     pullForce;

    [Header("Pull")]
    public float radiusMultiplier = 0.5f;

    float PullRadius => transform.localScale.x * radiusMultiplier;

    int beatCount;

    void Start()
    {
        BeatConductor.OnBeat += OnBeat;
    }

    void OnDestroy()
    {
        BeatConductor.OnBeat -= OnBeat;
    }

    void FixedUpdate()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, PullRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            hit.GetComponent<MobEnemyMovement>()?.SetPull(transform.position, pullForce);
            hit.GetComponent<ExplodingEnemyMovement>()?.SetPull(transform.position, pullForce);
        }
    }

    void OnBeat()
    {
        if (Time.timeScale == 0) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, PullRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(damagePerBeat, weaponType);
        }

        beatCount++;
        if (beatCount >= lifetimeBeats)
            Destroy(gameObject);
    }
}
