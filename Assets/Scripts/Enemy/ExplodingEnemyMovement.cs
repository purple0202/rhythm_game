using UnityEngine;
using System.Collections;

public class ExplodingEnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;

    [Header("Ranges")]
    public float detectionRange = 5f;
    public float explosionTriggerRange = 1f;

    [Header("Level-Up Push")]
    [SerializeField] float pushDistance = 1f;
    [SerializeField] float pushRadius = 5f;
    [SerializeField] float pushDuration = 0.8f;

    [Header("Explosion")]
    public float explosionDamageRadius = 2.5f;
    public float playerDamage = 25f;
    public float enemyDamage = 15f;
    public LayerMask enemyLayer;

    Transform player;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;

    bool    hasExploded = false;
    bool    isRunning   = false;
    Vector2 pullVelocity;

    public void SetPull(Vector2 center, float force)
    {
        pullVelocity = ((Vector2)center - (Vector2)transform.position).normalized * force;
    }

    void OnEnable()  => LevelSystem.OnLevelUp += OnLevelUp;
    void OnDisable() => LevelSystem.OnLevelUp -= OnLevelUp;

    void OnLevelUp()
    {
        if (player == null || hasExploded) return;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > pushRadius) return;
        Push(player.position, pushDistance, pushDuration);
    }

    public void Push(Vector2 origin, float distance, float duration)
    {
        if (hasExploded) return;
        Vector2 dir = ((Vector2)transform.position - origin).normalized;
        if (dir == Vector2.zero) dir = Vector2.up;
        Vector3 target = transform.position + (Vector3)(dir * distance);
        StartCoroutine(PushCoroutine(transform.position, target, duration));
    }

    IEnumerator PushCoroutine(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.position = to;
        if (rb != null) rb.position = to;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            Explode();
    }

    void Update()
    {
        if (player == null || hasExploded) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= explosionTriggerRange)
        {
            Explode();
            return;
        }

        isRunning = dist <= detectionRange;

        float dx = player.position.x - transform.position.x;
        if (dx != 0)
            spriteRenderer.flipX = dx < 0;
    }

    void FixedUpdate()
    {
        if (player == null || hasExploded)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * (isRunning ? runSpeed : walkSpeed) + pullVelocity;
        pullVelocity = Vector2.zero;

        if (animator != null)
            animator.SetBool("IsRunning", isRunning);

        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10);
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        StartCoroutine(ExplodeSequence());

        // TakeDamage(99999) triggers Die() in EnemyHealth, which handles XP + unregister,
        // then returns early (no Destroy) because hasExploded is true
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(99999f);
    }

    // Called by EnemyHealth.Die() when the player kills this enemy
    public void OnDeathExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;
        StartCoroutine(ExplodeSequence());
        // Die() returns early without Destroy — coroutine handles it
    }

    IEnumerator ExplodeSequence()
    {
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetBool("IsExploding", true);

        DealExplosionDamage();

        if (animator != null)
        {
            yield return null;
            while (animator.IsInTransition(0))
                yield return null;
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        Destroy(gameObject);
    }

    void DealExplosionDamage()
    {
        if (player != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer <= explosionDamageRadius)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.TakeDamage(playerDamage);
            }
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionDamageRadius, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            if (eh != null)
                eh.TakeDamage(enemyDamage);
        }
    }
}
