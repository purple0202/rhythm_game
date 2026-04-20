using UnityEngine;

public class RangedEnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;

    [Header("Attack")]
    public float fireRange = 5f;
    public float castTime = 1f;
    public float fireCooldown = 2f;
    public GameObject projectilePrefab;

    Transform player;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;

    enum State { Chasing, Casting, Cooldown }
    State state = State.Chasing;
    float stateTimer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Chasing:
                if (dist <= fireRange)
                    EnterCasting();
                break;

            case State.Casting:
                // If the player runs out of range, cancel the cast
                if (dist > fireRange)
                {
                    EnterChasing();
                    break;
                }
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    Fire();
                break;

            case State.Cooldown:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    if (dist <= fireRange)
                        EnterCasting();
                    else
                        EnterChasing();
                }
                break;
        }

        // Flip sprite horizontally to always face the player
        float dx = player.position.x - transform.position.x;
        if (dx != 0)
            spriteRenderer.flipX = dx < 0;
    }

    void FixedUpdate()
    {
        if (state == State.Chasing)
        {
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", state == State.Chasing);
            animator.SetBool("IsCasting", state == State.Casting);
        }
    }

    void EnterChasing()
    {
        state = State.Chasing;
    }

    void EnterCasting()
    {
        state = State.Casting;
        stateTimer = castTime;
    }

    void Fire()
    {
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null)
            {
                Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                ep.SetDirection(dir);
            }
        }

        state = State.Cooldown;
        stateTimer = fireCooldown;
    }
}
