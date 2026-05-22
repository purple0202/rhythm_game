using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cyclone : MonoBehaviour
{
    [Header("Movement")]
    public float startSpeed = 2f;
    public float deceleration = 0.4f;
    public float rotationSpeed = 200f;

    [Header("Fade")]
    public float fadeDuration = 0.6f;

    [Header("Detection")]
    public LayerMask enemyLayer;

    // Set by CycloneWeapon at spawn
    [HideInInspector] public float damage;
    [HideInInspector] public int lifetimeBeats;
    [HideInInspector] public EnemyType weaponType;
    [HideInInspector] public List<DotType> dotApplications = new();

    private float currentSpeed;
    private int beatsElapsed;
    private bool isFading;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentSpeed = startSpeed;
        BeatConductor.OnBeat += OnBeat;
    }

    void OnDestroy()
    {
        BeatConductor.OnBeat -= OnBeat;
    }

    void OnBeat()
    {
        if (Time.timeScale == 0 || isFading) return;

        DealDamage();

        beatsElapsed++;
        if (beatsElapsed >= lifetimeBeats)
        {
            isFading = true;
            StartCoroutine(FadeAndDestroy());
        }
    }

    void Update()
    {
        if (isFading) return;

        // Decelerate
        currentSpeed = Mathf.Max(0f, currentSpeed - deceleration * Time.deltaTime);

        // Track nearest enemy
        if (currentSpeed > 0f)
        {
            GameObject nearest = FindNearest();
            if (nearest != null)
            {
                Vector2 dir = (nearest.transform.position - transform.position).normalized;
                transform.position += (Vector3)(dir * currentSpeed * Time.deltaTime);
            }
        }

        // Spin
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void DealDamage()
    {
        float radius = transform.localScale.x * 0.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            EnemyHealth health = hit.GetComponent<EnemyHealth>();
            if (health == null) continue;

            float mult = (ComboSystem.Instance != null ? ComboSystem.Instance.GetCurrentMultiplier() : 1f)
                       * (PassiveManager.Instance != null ? PassiveManager.Instance.GetDamageMultiplier() : 1f);
            health.TakeDamage(damage * mult, weaponType);

            if (dotApplications.Count > 0)
            {
                DotReceiver dr = hit.GetComponent<DotReceiver>();
                if (dr != null) foreach (var dot in dotApplications) dr.Apply(dot);
            }
        }
    }

    private GameObject FindNearest()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float bestDist = float.MaxValue;
        foreach (var e in enemies)
        {
            float d = (e.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; nearest = e; }
        }
        return nearest;
    }

    private IEnumerator FadeAndDestroy()
    {
        BeatConductor.OnBeat -= OnBeat;

        Color c = spriteRenderer != null ? spriteRenderer.color : Color.white;
        float startAlpha = c.a;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (spriteRenderer != null)
            {
                c.a = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
                spriteRenderer.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
