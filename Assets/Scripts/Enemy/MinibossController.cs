using UnityEngine;

public class MinibossController : MonoBehaviour
{
    public enum BeatPattern { TwoBeat = 2, ThreeBeat = 3, FourBeat = 4 }

    [Header("Identity")]
    [SerializeField] string bossName = "UNNAMED";

    [Header("Pattern")]
    public BeatPattern pattern = BeatPattern.ThreeBeat;

    [Header("References")]
    public BossIndicator indicator;

    Transform player;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;

    int beatIndex;

    void OnDisable() => BeatConductor.OnBeat -= OnBeat;

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("MinibossController: no GameObject tagged 'Player' found.");
            OnIntroComplete();
            return;
        }

        player = playerGO.transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        beatIndex = 0;
        indicator?.Setup((int)pattern);

        if (BossIntroUI.Instance != null)
            BossIntroUI.Instance.Play(bossName, spriteRenderer?.sprite, OnIntroComplete);
        else
            OnIntroComplete();
    }

    void OnIntroComplete()
    {
        BeatConductor.OnBeat += OnBeat;
    }

    void OnBeat()
    {
        int length = (int)pattern;
        indicator?.OnBeat(beatIndex, length);

        if (beatIndex == length - 1)
        {
            ExecuteAttack();
            beatIndex = 0;
        }
        else
        {
            ExecuteCharge();
            beatIndex++;
        }
    }

    void ExecuteCharge()
    {
        // TODO: per-pattern charge behavior
    }

    void ExecuteAttack()
    {
        // TODO: per-pattern attack behavior
    }

    public void SetPattern(BeatPattern newPattern)
    {
        pattern = newPattern;
        beatIndex = 0;
        indicator?.Setup((int)newPattern);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10);
        }
    }
}
