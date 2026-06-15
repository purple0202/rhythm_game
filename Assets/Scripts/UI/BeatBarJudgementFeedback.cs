using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BeatBarJudgementFeedback : MonoBehaviour
{
    [Header("Border Image")]
    [SerializeField] Image borderImage;
    [SerializeField] Color idleColor    = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] Color perfectColor = new Color(0.3f, 1f, 0.4f,  1f);
    [SerializeField] Color goodColor    = new Color(1f,   0.9f, 0.2f, 1f);
    [SerializeField] Color badColor     = new Color(1f,   0.2f, 0.2f, 1f);
    [SerializeField] float flashDuration = 0.25f;

    [Header("Bad Ping")]
    [SerializeField] GameObject badPingObject;

    Coroutine flashCoroutine;

    void OnEnable()  => InputJudge.OnJudgement += OnJudgement;
    void OnDisable() => InputJudge.OnJudgement -= OnJudgement;

    void Start()
    {
        if (borderImage != null)   borderImage.color = idleColor;
        if (badPingObject != null) badPingObject.SetActive(false);
    }

    void OnJudgement(string judgement)
    {
        Color target;
        switch (judgement)
        {
            case "Perfect": target = perfectColor; break;
            case "Good":    target = goodColor;    break;
            case "Bad":     target = badColor;     break;
            default:        return; // Auto — no visual change
        }

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashCoroutine(target));

        if (judgement == "Bad" && badPingObject != null)
        {
            // Toggle off then on to restart the animation from the beginning
            badPingObject.SetActive(false);
            badPingObject.SetActive(true);
        }
    }

    IEnumerator FlashCoroutine(Color flashColor)
    {
        if (borderImage == null) yield break;

        borderImage.color = flashColor;
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            borderImage.color = Color.Lerp(flashColor, idleColor, elapsed / flashDuration);
            yield return null;
        }

        borderImage.color = idleColor;
        flashCoroutine = null;
    }
}
