using UnityEngine;
using System.Collections.Generic;

public class Dagger : MonoBehaviour
{
    [HideInInspector] public float         damage;
    [HideInInspector] public float         speed;
    [HideInInspector] public float         lifetime;
    [HideInInspector] public Vector2       direction;
    [HideInInspector] public EnemyType     weaponType;
    [HideInInspector] public List<DotType> dotApplications = new();

    [Header("Visual")]
    public float rotationSpeed = 0f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        if (rotationSpeed != 0f)
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null) return;

        enemy.TakeDamage(damage, weaponType);

        if (dotApplications.Count > 0)
        {
            DotReceiver dr = other.GetComponent<DotReceiver>();
            if (dr != null) foreach (var dot in dotApplications) dr.Apply(dot);
        }

        Destroy(gameObject);
    }
}
