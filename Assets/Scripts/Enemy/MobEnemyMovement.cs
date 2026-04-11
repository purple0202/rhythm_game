using UnityEngine;

public class MobEnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;

    Transform player;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;

    Vector2 movement;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        movement = (player.position - transform.position).normalized;
        rb.linearVelocity = movement * moveSpeed;

        // Flip sprite to face the direction of movement
        if (movement.x != 0)
            spriteRenderer.flipX = movement.x < 0;

        if (animator != null)
            animator.SetBool("IsMoving", rb.linearVelocity.sqrMagnitude > 0.01f);
    }
}
