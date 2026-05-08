using System.Collections;
using UnityEngine;
using TMPro;

public class ComboUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI comboText;
    [SerializeField] ParticleSystem  flameParticles;
    [SerializeField] RectTransform   punchTarget;

    [Header("Punch Animation")]
    [SerializeField] float punchScale    = 1.35f;
    [SerializeField] float punchDuration = 0.1f;

    void OnEnable()
    {
        ComboSystem.OnComboChanged += HandleComboChanged;
        ComboSystem.OnComboReset   += HandleComboReset;
    }

    void OnDisable()
    {
        ComboSystem.OnComboChanged -= HandleComboChanged;
        ComboSystem.OnComboReset   -= HandleComboReset;
    }

    void Start()
    {
        punchTarget.gameObject.SetActive(false);
    }

    void HandleComboChanged(int combo)
    {
        if (combo <= 0)
        {
            punchTarget.gameObject.SetActive(false);
            return;
        }

        punchTarget.gameObject.SetActive(true);
        comboText.text = $"x{combo}";

        ComboTier tier = ComboSystem.Instance.GetCurrentTier();
        comboText.color = tier.displayColor;

        if (flameParticles != null)
        {
            var emission = flameParticles.emission;
            emission.rateOverTime = tier.emissionRate;

            if (tier.emissionRate > 0 && !flameParticles.isPlaying)
                flameParticles.Play();
            else if (tier.emissionRate <= 0)
                flameParticles.Stop();
        }

        StopAllCoroutines();
        StartCoroutine(PunchScale());
    }

    void HandleComboReset()
    {
        if (flameParticles != null)
            flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    IEnumerator PunchScale()
    {
        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / punchDuration;
            punchTarget.localScale = Vector3.one * Mathf.Lerp(punchScale, 1f, t);
            yield return null;
        }
        punchTarget.localScale = Vector3.one;
    }
}
