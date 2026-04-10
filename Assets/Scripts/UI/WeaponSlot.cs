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

    public Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private bool hasWeapon = false;

    public void SetWeapon(Sprite weaponIcon)
    {
        hasWeapon = weaponIcon != null;
        if (weaponIcon != null)
        {   
            Debug.Log("HAS WEAPON");
            icon.sprite = weaponIcon;
            // SetAlpha(selectedAlpha);
            icon.color = new Color(1f, 1f, 1f, unselectedAlpha);
        }
        else
        {
            Debug.Log("nahh didn't work");
            icon.sprite = null;
            icon.color = emptyColor;
            // SetAlpha(0f);
        }
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
}
