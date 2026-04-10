using UnityEngine;

public class CannonWeapon : Weapon
{
    [Header("Damage")]
    public float perfectDamage = 60f;
    public float goodDamage = 40f;
    public float autoDamage = 20f;

    [Header("Projectile")]
    public GameObject cannonballPrefab;

    [Header("Siege Mode")]
    public PlayerMovement playerMovement;
    public PlayerDash playerDash;
    public SpriteRenderer playerRenderer;
    public Sprite tankSprite;

    private Sprite originalSprite;
    private bool originalSpriteCached = false;
    private Vector2 aimDirection = Vector2.right;

    void OnEnable()
    {
        // Cache the normal sprite the first time siege mode is entered
        if (!originalSpriteCached && playerRenderer != null)
        {
            originalSprite = playerRenderer.sprite;
            originalSpriteCached = true;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            // Stop any leftover momentum
            Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (playerDash != null) playerDash.enabled = false;
        if (playerRenderer != null && tankSprite != null) playerRenderer.sprite = tankSprite;
    }

    void OnDisable()
    {
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerDash != null) playerDash.enabled = true;
        if (playerRenderer != null && originalSpriteCached) playerRenderer.sprite = originalSprite;
    }

    void Update()
    {
        // WASD now controls aim direction instead of movement
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(x, y);
        if (input.sqrMagnitude > 0.01f)
            aimDirection = input.normalized;
    }

    public override void PerformAttack(string judgement)
    {
        if (judgement == "Bad") return;
        if (cannonballPrefab == null) return;

        float damage = judgement == "Perfect" ? perfectDamage :
                       judgement == "Good"    ? goodDamage    : autoDamage;

        GameObject proj = Instantiate(cannonballPrefab, transform.position, Quaternion.identity);
        CannonBall ball = proj.GetComponent<CannonBall>();
        if (ball != null)
        {
            ball.SetDirection(aimDirection);
            ball.damage = damage;
        }
    }
}
