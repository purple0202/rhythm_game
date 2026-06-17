using UnityEngine;
using System.Collections;

public class MobEnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;

    [Header("Per-Type Move Speed")]
    [SerializeField] float blueMoveSpeed = 2f;
    [SerializeField] float redMoveSpeed = 2f;
    [SerializeField] float greenMoveSpeed = 2f;
    [SerializeField] float yellowMoveSpeed = 2f;

    [Header("Per-Type Animator Controller")]
    [SerializeField] RuntimeAnimatorController noneController;
    [SerializeField] RuntimeAnimatorController blueController;
    [SerializeField] RuntimeAnimatorController redController;
    [SerializeField] RuntimeAnimatorController greenController;
    [SerializeField] RuntimeAnimatorController yellowController;

    float currentMoveSpeed;

    Transform player;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;
    DotReceiver dotReceiver;
    EnemyHealth enemyHealth;

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
        enemyHealth   = GetComponent<EnemyHealth>();

        // EnemyHealth.SetType() is called by the spawner right after Instantiate, so by the
        // time Start() runs here the type is already assigned.
        ApplyType(enemyHealth != null ? enemyHealth.enemyType : EnemyType.None);

        if (animator != null)
            animator.Play(0, 0, Random.value);
    }

    void ApplyType(EnemyType type)
    {
        currentMoveSpeed = type switch
        {
            EnemyType.Blue   => blueMoveSpeed,
            EnemyType.Red    => redMoveSpeed,
            EnemyType.Green  => greenMoveSpeed,
            EnemyType.Yellow => yellowMoveSpeed,
            _                => moveSpeed
        };

        RuntimeAnimatorController controller = type switch
        {
            EnemyType.Blue   => blueController,
            EnemyType.Red    => redController,
            EnemyType.Green  => greenController,
            EnemyType.Yellow => yellowController,
            _                => noneController
        };
        if (animator != null && controller != null)
            animator.runtimeAnimatorController = controller;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        movement = (player.position - transform.position).normalized;
        float slow = dotReceiver != null ? dotReceiver.slowMultiplier : 1f;
        rb.linearVelocity = movement * currentMoveSpeed * slow + pullVelocity;
        pullVelocity = Vector2.zero;

        if (movement.x != 0)
            spriteRenderer.flipX = movement.x < 0;

        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10);
    }
}
