using UnityEngine;
using System.Collections;

public class MobEnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;

    Transform player;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;
    DotReceiver dotReceiver;

    [Header("Level-Up Push")]
    [SerializeField] float pushDistance = 1f;
    [SerializeField] float pushRadius = 5f;
    [SerializeField] float pushDuration = 0.8f;

    Vector2 movement;
    Vector2 pullVelocity;

    public void SetPull(Vector2 center, float force)
    {
        pullVelocity = ((Vector2)center - (Vector2)transform.position).normalized * force;
    }

    void OnEnable()  => LevelSystem.OnLevelUp += OnLevelUp;
    void OnDisable() => LevelSystem.OnLevelUp -= OnLevelUp;

    void OnLevelUp()
    {
        if (player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > pushRadius) return;
        Push(player.position, pushDistance, pushDuration);
    }

    public void Push(Vector2 origin, float distance, float duration)
    {
        Vector2 dir = ((Vector2)transform.position - origin).normalized;
        if (dir == Vector2.zero) dir = Vector2.up;
        Vector3 target = transform.position + (Vector3)(dir * distance);
        StartCoroutine(PushCoroutine(transform.position, target, duration));
    }

    IEnumerator PushCoroutine(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.position = to;
        rb.position = to;
    }

    void Start()
    {
        player        = GameObject.FindGameObjectWithTag("Player").transform;
        rb            = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator      = GetComponent<Animator>();
        dotReceiver   = GetComponent<DotReceiver>();

        if (animator != null)
            animator.Play(0, 0, Random.value);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        movement = (player.position - transform.position).normalized;
        float slow = dotReceiver != null ? dotReceiver.slowMultiplier : 1f;
        rb.linearVelocity = movement * moveSpeed * slow + pullVelocity;
        pullVelocity = Vector2.zero;

        if (movement.x != 0)
            spriteRenderer.flipX = movement.x < 0;

        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10);
    }
}
