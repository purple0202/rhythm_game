using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BeatBarJudgementFeedback : MonoBehaviour
{
    [Header("Border Image")]
    [SerializeField] Image borderImage;
    [SerializeField] Sprite idleSprite;
    [SerializeField] Sprite perfectSprite;
    [SerializeField] Sprite goodSprite;
    [SerializeField] Sprite badSprite;
    [SerializeField] float flashDuration = 0.25f;

    [Header("Bad Ping")]
    [SerializeField] GameObject badPingObject;
    [SerializeField] float badPingDuration = 1f;

    Coroutine returnCoroutine;
    Coroutine pingCoroutine;

    void OnEnable()  => InputJudge.OnJudgement += OnJudgement;
    void OnDisable() => InputJudge.OnJudgement -= OnJudgement;

    void Start()
    {
        if (borderImage != null)   borderImage.sprite = idleSprite;
        if (badPingObject != null) badPingObject.SetActive(false);
    }

    void OnJudgement(string judgement)
    {
        Sprite target;
        switch (judgement)
        {
            case "Perfect": target = perfectSprite; break;
            case "Good":    target = goodSprite;    break;
            case "Bad":     target = badSprite;     break;
            default:        return;
        }

        if (borderImage != null) borderImage.sprite = target;

        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(ReturnToIdle());

        if (judgement == "Bad" && badPingObject != null)
        {
            if (pingCoroutine != null) StopCoroutine(pingCoroutine);
            pingCoroutine = StartCoroutine(PlayPing());
        }
    }

    IEnumerator PlayPing()
    {
        badPingObject.SetActive(false);
        badPingObject.SetActive(true);
        yield return new WaitForSecondsRealtime(badPingDuration);
        badPingObject.SetActive(false);
        pingCoroutine = null;
    }

    IEnumerator ReturnToIdle()
    {
        yield return new WaitForSecondsRealtime(flashDuration);
        if (borderImage != null) borderImage.sprite = idleSprite;
        returnCoroutine = null;
    }
}
