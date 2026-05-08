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

    [Header("Tiers")]
    [SerializeField] ComboTier[] tiers;

    [System.Serializable]
    struct ComboTier
    {
        public int   minCombo;
        public Color baseColor;
        [Range(1f, 8f)]
        public float hdrIntensity;   // multiplied into color — values >1 trigger Bloom
        public float emissionRate;
    }

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

        ComboTier tier = GetTier(combo);

        // HDR color: multiplying above 1 makes Bloom pick this up
        comboText.color = tier.baseColor * tier.hdrIntensity;

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

    ComboTier GetTier(int combo)
    {
        ComboTier result = tiers.Length > 0 ? tiers[0] : default;
        foreach (var tier in tiers)
            if (combo >= tier.minCombo) result = tier;
        return result;
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
