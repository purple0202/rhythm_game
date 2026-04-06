using UnityEngine;
using System.Collections;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashDistance = 3f;
    public float dashDuration = 0.2f;

    [Header("Cooldown & Charges")]
    public float dashCooldown = 1f;
    public int maxCharges = 2;

    int currentCharges;
    float lastDashTime;

    [Header("Collision")]
    public LayerMask obstacleLayer;
    public LayerMask enemyLayer;

    Rigidbody2D rb;
    PlayerHealth playerHealth;

    bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();

        currentCharges = maxCharges;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryDash();
        }

        RechargeDash();
    }

    void TryDash()
    {
        if (isDashing) return;
        if (currentCharges <= 0) return;

        Vector2 direction = GetMovementDirection();

        if (direction == Vector2.zero)
            direction = Vector2.right; // fallback

        StartCoroutine(Dash(direction));
    }

    Vector2 GetMovementDirection()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        return new Vector2(x, y).normalized;
    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        currentCharges--;

        playerHealth.SetInvincible(true);

        float startTime = Time.time;
        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + direction * dashDistance;

        // Disable collisions with enemies
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            true
        );

        while (Time.time < startTime + dashDuration)
        {
            float t = (Time.time - startTime) / dashDuration;

            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);
            rb.MovePosition(newPos);

            yield return null;
        }

        rb.MovePosition(targetPos);

        // Re-enable collisions
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            false
        );

        ResolveOverlap();

        playerHealth.SetInvincible(false);

        lastDashTime = Time.time;
        isDashing = false;
    }

    void RechargeDash()
    {
        if (currentCharges >= maxCharges) return;

        if (Time.time >= lastDashTime + dashCooldown)
        {
            currentCharges++;
            lastDashTime = Time.time;
        }
    }

    void ResolveOverlap()
    {
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(
            transform.position,
            0.5f,
            enemyLayer
        );

        foreach (var col in overlaps)
        {
            Rigidbody2D enemyRb = col.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                Vector2 dir = (col.transform.position - transform.position).normalized;
                enemyRb.AddForce(dir * 5f, ForceMode2D.Impulse);
            }
        }
    }
}