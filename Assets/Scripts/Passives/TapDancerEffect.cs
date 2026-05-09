using System.Collections;
using UnityEngine;

public class TapDancerEffect : PassiveEffect
{
    [Header("Speed Settings")]
    public float speedPerPerfect    = 0.1f;
    public float maxSpeedMultiplier = 2.5f;
    public int   buffDurationBeats  = 3;

    [Header("Afterimage")]
    public float ghostInterval = 0.05f;
    public float ghostDuration = 0.3f;
    public Color ghostColor    = new Color(0.5f, 0.8f, 1f, 0.5f);

    PlayerMovement playerMovement;
    SpriteRenderer playerSprite;
    float originalMoveSpeed;
    int   maxStacks;
    int   stackCount;
    int   beatsRemaining;
    Coroutine ghostCoroutine;

    public override void OnActivate()
    {
        playerMovement    = GetComponent<PlayerMovement>();
        playerSprite      = GetComponentInChildren<SpriteRenderer>();
        originalMoveSpeed = playerMovement != null ? playerMovement.moveSpeed : 5f;
        maxStacks         = Mathf.FloorToInt((originalMoveSpeed * maxSpeedMultiplier - originalMoveSpeed) / speedPerPerfect);

        InputJudge.OnJudgement += OnJudgement;
        BeatConductor.OnBeat   += OnBeat;

        NotifyActiveState(false);
        NotifyStackCount("");
    }

    void OnDisable()
    {
        InputJudge.OnJudgement -= OnJudgement;
        BeatConductor.OnBeat   -= OnBeat;
        StopGhostTrail();
        RemoveBonus();
    }

    void OnJudgement(string judgement)
    {
        if (judgement != "Perfect") return;
        if (Time.timeScale == 0f) return;

        stackCount     = Mathf.Min(stackCount + 1, maxStacks);
        beatsRemaining = buffDurationBeats;

        ApplyBonus();
        StartGhostTrail();
        NotifyActiveState(true);
        NotifyStackCount(stackCount.ToString());
    }

    void OnBeat()
    {
        if (Time.timeScale == 0f) return;
        if (beatsRemaining <= 0) return;

        beatsRemaining--;
        if (beatsRemaining > 0) return;

        stackCount = 0;
        RemoveBonus();
        StopGhostTrail();
        NotifyActiveState(false);
        NotifyStackCount("");
    }

    void ApplyBonus()
    {
        if (playerMovement == null) return;
        float maxSpeed = originalMoveSpeed * maxSpeedMultiplier;
        playerMovement.moveSpeed = Mathf.Min(originalMoveSpeed + stackCount * speedPerPerfect, maxSpeed);
    }

    void RemoveBonus()
    {
        if (playerMovement == null) return;
        playerMovement.moveSpeed = originalMoveSpeed;
    }

    void StartGhostTrail()
    {
        if (ghostCoroutine != null) return;
        ghostCoroutine = StartCoroutine(GhostTrail());
    }

    void StopGhostTrail()
    {
        if (ghostCoroutine == null) return;
        StopCoroutine(ghostCoroutine);
        ghostCoroutine = null;
    }

    IEnumerator GhostTrail()
    {
        while (true)
        {
            SpawnGhost();
            yield return new WaitForSeconds(ghostInterval);
        }
    }

    void SpawnGhost()
    {
        if (playerSprite == null || playerSprite.sprite == null) return;

        GameObject ghost = new GameObject("TapDancerGhost");
        ghost.transform.position   = transform.position;
        ghost.transform.localScale = playerSprite.transform.lossyScale;

        SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
        sr.sprite       = playerSprite.sprite;
        sr.flipX        = playerSprite.flipX;
        sr.sortingOrder = playerSprite.sortingOrder - 1;
        sr.color        = ghostColor;

        StartCoroutine(FadeGhost(sr, ghostDuration));
        Destroy(ghost, ghostDuration + 0.1f);
    }

    IEnumerator FadeGhost(SpriteRenderer sr, float duration)
    {
        Color start   = sr.color;
        float elapsed = 0f;
        while (elapsed < duration && sr != null)
        {
            elapsed      += Time.deltaTime;
            float t       = elapsed / duration;
            sr.color      = new Color(start.r, start.g, start.b, start.a * (1f - t));
            yield return null;
        }
    }
}
