using UnityEngine;
using System.Collections.Generic;

public class Boomerang : MonoBehaviour
{
    [HideInInspector] public float      outboundDamage;
    [HideInInspector] public float      returnDamage;
    [HideInInspector] public float      maxRange;
    [HideInInspector] public float      speed;
    [HideInInspector] public float      returnSpeed;
    [HideInInspector] public bool       allowDoubleDamage;
    [HideInInspector] public Vector2    direction;
    [HideInInspector] public EnemyType  weaponType;
    [HideInInspector] public List<DotType> dotApplications = new();
    [HideInInspector] public Transform  playerTransform;

    [Header("Visual")]
    public float rotationSpeed = 720f;

    enum Phase { Outbound, Returning }
    Phase phase = Phase.Outbound;

    float distanceTraveled;
    readonly HashSet<Collider2D> hitSet = new();

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        if (phase == Phase.Outbound)
        {
            float step = speed * Time.deltaTime;
            transform.position += (Vector3)(direction * step);
            distanceTraveled   += step;

            if (distanceTraveled >= maxRange)
                SwitchToReturn();
        }
        else
        {
            if (playerTransform == null) { Destroy(gameObject); return; }

            Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
            float   step     = returnSpeed * Time.deltaTime;

            if (toPlayer.magnitude <= step)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += (Vector3)(toPlayer.normalized * step);
        }
    }

    void SwitchToReturn()
    {
        phase = Phase.Returning;
        if (allowDoubleDamage) hitSet.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (hitSet.Contains(other)) return;

        hitSet.Add(other);

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null) return;

        float dmg = phase == Phase.Outbound ? outboundDamage : returnDamage;
        enemy.TakeDamage(dmg, weaponType);

        if (dotApplications.Count > 0)
        {
            DotReceiver dr = other.GetComponent<DotReceiver>();
            if (dr != null) foreach (var dot in dotApplications) dr.Apply(dot);
        }
    }
}
