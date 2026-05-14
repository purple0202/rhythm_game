using UnityEngine;
using UnityEngine.UI;

public class ShieldBar : MonoBehaviour
{
    Slider slider;
    CanvasGroup canvasGroup;

    void Awake()
    {
        slider = GetComponent<Slider>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()  => PlayerHealth.OnShieldChanged += OnShieldChanged;
    void OnDisable() => PlayerHealth.OnShieldChanged -= OnShieldChanged;

    void OnShieldChanged(float current, float max)
    {
        slider.maxValue = max;
        slider.value    = current;
        canvasGroup.alpha = current > 0f ? 1f : 0f;
    }
}
