using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    [Header("Push Settings")]
    public float pushRadius = 1.0f;
    public float pushStrength = 5f;

    [Header("Resistance")]
    [Tooltip("0 = no resistance, 1 = very strong resistance")]
    [Range(0f, 1f)]
    public float resistance = 0.2f;

    public LayerMask enemyLayer;

    void FixedUpdate()
    {
        ApplyPush();
    }

    void ApplyPush()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            pushRadius,
            enemyLayer
        );

        foreach (var col in enemies)
        {
            Rigidbody2D enemyRb = col.GetComponent<Rigidbody2D>();
            if (enemyRb == null) continue;

            Vector2 dir = (col.transform.position - transform.position);

            float distance = dir.magnitude;

            if (distance == 0) continue;

            dir.Normalize();

            // Push force
            float force = pushStrength * (1f - resistance);

            enemyRb.AddForce(dir * force, ForceMode2D.Force);
        }
    }
}