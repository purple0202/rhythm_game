using UnityEngine;
using System.Collections.Generic;

public class CannonBall : MonoBehaviour
{
    public float speed = 5f;
    public float damage = 30f;
    public float lifetime = 4f;

    public EnemyType weaponType = EnemyType.None;

    private Vector2 direction;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    public void SetDirection(Vector2 dir) => direction = dir.normalized;

    void Start() => Destroy(gameObject, lifetime);

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;
        if (hitEnemies.Contains(collision)) return;

        hitEnemies.Add(collision);
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            float multiplier = (ComboSystem.Instance != null ? ComboSystem.Instance.GetCurrentMultiplier() : 1f)
                             * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f);
            enemy.TakeDamage(damage * multiplier, weaponType);
        }
        // No Destroy — pierces through all enemies
    }
}
