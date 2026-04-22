using System;
using UnityEngine;
using TMPro;

public class EnemyDamageMeter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI damageText;

    public static bool IsVisible = true;
    static event Action<bool> OnVisibilityChanged;

    Canvas canvas;

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

    void ApplyVisibility(bool visible)
    {
        canvas.enabled = visible;
    }

    public static void SetVisible(bool visible)
    {
        IsVisible = visible;
        OnVisibilityChanged?.Invoke(visible);
    }

    public void UpdateDisplay(float totalDamage)
    {
        damageText.text = Mathf.RoundToInt(totalDamage).ToString();
    }
}
