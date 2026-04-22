using System;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI damageText;
    [SerializeField] float floatSpeed = 1.5f;
    [SerializeField] float lifetime = 0.8f;

    public static bool IsVisible = true;
    static event Action<bool> OnVisibilityChanged;

    Canvas canvas;
    float elapsed;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    void OnEnable()
    {
        OnVisibilityChanged += ApplyVisibility;
        ApplyVisibility(IsVisible);
    }

    void OnDisable()
    {
        OnVisibilityChanged -= ApplyVisibility;
    }

    void ApplyVisibility(bool visible) => canvas.enabled = visible;

    public static void SetVisible(bool visible)
    {
        IsVisible = visible;
        OnVisibilityChanged?.Invoke(visible);
    }

    public void Setup(float damage)
    {
        damageText.text = Mathf.RoundToInt(damage).ToString();
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        float alpha = Mathf.Lerp(1f, 0f, elapsed / lifetime);
        Color c = damageText.color;
        c.a = alpha;
        damageText.color = c;

        if (elapsed >= lifetime)
            Destroy(gameObject);
    }
}
