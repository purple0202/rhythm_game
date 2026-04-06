using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
    public Image icon;

    [Header("Visual Settings")]
    public float selectedScale = 1.2f;
    public float normalScale = 1f;

    public float selectedAlpha = 1f;
    public float unselectedAlpha = 0.5f;

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            transform.localScale = Vector3.one * selectedScale;
            SetAlpha(selectedAlpha);
        }
        else
        {
            transform.localScale = Vector3.one * normalScale;
            SetAlpha(unselectedAlpha);
        }
    }

    void SetAlpha(float alpha)
    {
        Color c = icon.color;
        c.a = alpha;
        icon.color = c;
    }
}