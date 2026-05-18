using UnityEngine;
using System.Collections.Generic;

public class BlackHoleProjectile : MonoBehaviour
{
    [HideInInspector] public float         damage;
    [HideInInspector] public float         speed;
    [HideInInspector] public float         lifetime;
    [HideInInspector] public EnemyType     weaponType;
    [HideInInspector] public List<DotType> dotApplications = new();
    [HideInInspector] public bool          isMaxCharge;
    [HideInInspector] public GameObject    blackHolePrefab;
    [HideInInspector] public float         blackHoleDamagePerBeat;
    [HideInInspector] public int           blackHoleLifetimeBeats;
    [HideInInspector] public float         blackHolePullForce;

    bool       launched;
    Vector2    direction;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void Launch(Vector2 dir)
    {
        launched  = true;
        direction = dir;
        if (col != null) col.enabled = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!launched) return;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null) return;

        if (!isMaxCharge)
        {
            enemy.TakeDamage(damage, weaponType);

            if (dotApplications.Count > 0)
            {
                DotReceiver dr = other.GetComponent<DotReceiver>();
                if (dr != null) foreach (var dot in dotApplications) dr.Apply(dot);
            }

            Destroy(gameObject);
        }
        else
        {
            SpawnBlackHole();
        }
    }

    void SpawnBlackHole()
    {
        if (blackHolePrefab != null)
        {
            GameObject go = Instantiate(blackHolePrefab, transform.position, Quaternion.identity);
            go.transform.localScale = transform.localScale;

            BlackHole bh = go.GetComponent<BlackHole>();
            if (bh != null)
            {
                bh.damagePerBeat  = blackHoleDamagePerBeat;
                bh.lifetimeBeats  = blackHoleLifetimeBeats;
                bh.weaponType     = weaponType;
                bh.pullForce      = blackHolePullForce;
            }
        }

        Destroy(gameObject);
    }
}
