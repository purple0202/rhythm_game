using UnityEngine;

public class MobEnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;

    Transform player;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;

    Vector2 movement;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Start animation at a random point so enemies don't animate in sync
        if (animator != null)
            animator.Play(0, 0, Random.value);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        movement = (player.position - transform.position).normalized;
        rb.linearVelocity = movement * moveSpeed;

        // Flip sprite to face the direction of movement
        if (movement.x != 0)
            spriteRenderer.flipX = movement.x < 0;

        // Lower Y = closer to camera = higher sort order
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10);
    }
}
