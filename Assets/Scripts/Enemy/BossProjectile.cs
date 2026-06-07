using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossProjectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public bool isParriable;

    Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Launch(Vector2 direction, float speed)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        col.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        Destroy(gameObject);
    }

    void OnBecameInvisible() => Destroy(gameObject);
}
