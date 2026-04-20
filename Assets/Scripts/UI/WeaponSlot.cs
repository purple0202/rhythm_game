using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
    public Image icon;
    public Image outline;

    [Header("Visual Settings")]
    public float selectedScale = 1.2f;
    public float normalScale = 1f;

    public float selectedAlpha = 1f;
    public float unselectedAlpha = 0.5f;

    public Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private bool hasWeapon = false;

    static readonly Color BlueColor   = new Color(0.20f, 0.53f, 1.00f);
    static readonly Color RedColor    = new Color(1.00f, 0.22f, 0.22f);
    static readonly Color GreenColor  = new Color(0.18f, 0.85f, 0.35f);
    static readonly Color YellowColor = new Color(1.00f, 0.85f, 0.10f);

    public void SetWeapon(Sprite weaponIcon, EnemyType weaponType = EnemyType.None)
    {
        hasWeapon = weaponIcon != null;
        if (weaponIcon != null)
        {
            icon.sprite = weaponIcon;
            icon.color = new Color(1f, 1f, 1f, unselectedAlpha);
        }
        else
        {
            icon.sprite = null;
            icon.color = emptyColor;
        }

        SetOutlineColor(weaponType);
    }

    public void SetSelected(bool isSelected)
    {
        transform.localScale = Vector3.one * (isSelected ? selectedScale : normalScale);
        if (hasWeapon)
            SetAlpha(isSelected ? selectedAlpha : unselectedAlpha);
    }

    void SetAlpha(float alpha)
    {
        Color c = icon.color;
        c.a = alpha;
        icon.color = c;
    }

    void SetOutlineColor(EnemyType weaponType)
    {
        if (outline == null) return;

        Color color = weaponType switch
        {
            EnemyType.Blue   => BlueColor,
            EnemyType.Red    => RedColor,
            EnemyType.Green  => GreenColor,
            EnemyType.Yellow => YellowColor,
            _                => new Color(0.5f, 0.5f, 0.5f, 1f)
        };

        outline.color = color;
    }
}
