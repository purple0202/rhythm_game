using UnityEngine;

public class MobEnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;

    Transform player;
    Rigidbody2D rb;

    Vector2 movement;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        movement = (player.position - transform.position).normalized;
        rb.linearVelocity = movement * moveSpeed;
    }
}