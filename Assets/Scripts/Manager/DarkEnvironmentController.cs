using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DarkEnvironmentController : MonoBehaviour
{
    public static DarkEnvironmentController Instance { get; private set; }

    [Header("Lights")]
    [SerializeField] Light2D globalLight;
    [SerializeField] Light2D playerLight;

    [Header("Flicker")]
    [SerializeField] float flickerDuration = 2.5f;
    [SerializeField] float flickerIntervalMin = 0.04f;
    [SerializeField] float flickerIntervalMax = 0.18f;
    [SerializeField] float flickerLowIntensity = 0f;

    [Header("Fade to Dark")]
    [SerializeField] float fadeDuration = 0.8f;
    [SerializeField] float darkIntensity = 0f;

    float normalIntensity;

    void Awake()
    {
        Instance = this;
        if (globalLight != null)
            normalIntensity = globalLight.intensity;
        if (playerLight != null)
            playerLight.enabled = false;
    }

    public IEnumerator Trigger()
    {
        if (globalLight == null) yield break;

        float elapsed = 0f;
        while (elapsed < flickerDuration)
        {
            globalLight.intensity = Random.value > 0.5f ? normalIntensity : flickerLowIntensity;
            float wait = Random.Range(flickerIntervalMin, flickerIntervalMax);
            elapsed += wait;
            yield return new WaitForSeconds(wait);
        }

        float t = 0f;
        float startIntensity = globalLight.intensity;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            globalLight.intensity = Mathf.Lerp(startIntensity, darkIntensity, t / fadeDuration);
            yield return null;
        }
        globalLight.intensity = darkIntensity;

        if (playerLight != null)
            playerLight.enabled = true;
    }
}
