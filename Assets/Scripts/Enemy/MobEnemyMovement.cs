using UnityEngine;
using System.Collections;

public class MobEnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;

    Transform player;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;

    [Header("Level-Up Push")]
    [SerializeField] float pushDistance = 1f;
    [SerializeField] float pushRadius = 5f;
    [SerializeField] float pushDuration = 0.8f;

    Vector2 movement;

    void OnEnable()  => LevelSystem.OnLevelUp += OnLevelUp;
    void OnDisable() => LevelSystem.OnLevelUp -= OnLevelUp;

    void OnLevelUp()
    {
        if (player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > pushRadius) return;

        Vector2 dir = ((Vector2)transform.position - (Vector2)player.position).normalized;
        Vector3 target = transform.position + (Vector3)(dir * pushDistance);
        StartCoroutine(PushCoroutine(transform.position, target));
    }

    IEnumerator PushCoroutine(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        while (elapsed < pushDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / pushDuration));
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.position = to;
        rb.position = to;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (animator != null)
            animator.Play(0, 0, Random.value);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        movement = (player.position - transform.position).normalized;
        rb.linearVelocity = movement * moveSpeed;

        if (movement.x != 0)
            spriteRenderer.flipX = movement.x < 0;

        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10);
    }
}
