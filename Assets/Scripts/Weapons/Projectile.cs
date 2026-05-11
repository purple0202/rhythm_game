using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 3f;

    public EnemyType weaponType = EnemyType.None;
    public List<DotType> dotApplications = new();

    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                float multiplier = (ComboSystem.Instance != null ? ComboSystem.Instance.GetCurrentMultiplier() : 1f)
                                 * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f);
                enemy.TakeDamage(damage * multiplier, weaponType);
                if (dotApplications.Count > 0)
                {
                    DotReceiver dr = collision.GetComponent<DotReceiver>();
                    if (dr != null) foreach (var dot in dotApplications) dr.Apply(dot);
                }
            }

            Destroy(gameObject);
        }
    }
}