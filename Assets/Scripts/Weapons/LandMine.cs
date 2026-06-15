using UnityEngine;
using System.Collections.Generic;

public class LandMine : MonoBehaviour
{
    [HideInInspector] public float         damage;
    [HideInInspector] public EnemyType     weaponType;
    [HideInInspector] public List<DotType> dotApplications = new();
    [HideInInspector] public Vector2       target;
    [HideInInspector] public float         explosionRadius;
    [HideInInspector] public int           lifetimeBeats;

    [Header("Detection")]
    public float detectionRadius = 0.5f;

    [Header("Glide")]
    public float speedMultiplier = 8f;
    public float minGlideSpeed   = 0.5f;
    public float snapThreshold   = 0.05f;

    [Header("Visuals")]
    public Sprite flyingSprite;
    public Sprite activatedSprite;
    [SerializeField] float spinSpeed = 200f;

    SpriteRenderer   sr;
    CircleCollider2D col;
    bool  landed;
    int   beatCount;
    float startDist = -1f;

    void Awake()
    {
        col         = GetComponent<CircleCollider2D>();
        col.enabled = false;
        sr          = GetComponent<SpriteRenderer>();
        if (sr != null && flyingSprite != null) sr.sprite = flyingSprite;
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
    }

    void Update()
    {
        if (landed) return;

        float dist = Vector2.Distance(transform.position, target);

        if (startDist < 0f) startDist = dist;

        // spin decelerates proportionally as mine approaches target
        if (startDist > 0f)
            transform.Rotate(0f, 0f, spinSpeed * (dist / startDist) * Time.deltaTime);

        if (dist < snapThreshold)
        {
            transform.position = target;
            Land();
            return;
        }

        float speed = Mathf.Max(minGlideSpeed, dist * speedMultiplier);
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    void Land()
    {
        landed      = true;
        col.radius  = detectionRadius;
        col.enabled = true;
        if (sr != null && activatedSprite != null) sr.sprite = activatedSprite;
        BeatConductor.OnBeat += OnBeat;
    }

    void OnBeat()
    {
        if (Time.timeScale == 0) return;
        beatCount++;
        if (beatCount >= lifetimeBeats)
            Explode();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        Explode();
    }

    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null) continue;

            enemy.TakeDamage(damage, weaponType);

            if (dotApplications.Count > 0)
            {
                DotReceiver dr = hit.GetComponent<DotReceiver>();
                if (dr != null) foreach (var dot in dotApplications) dr.Apply(dot);
            }
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        BeatConductor.OnBeat -= OnBeat;
    }
}
