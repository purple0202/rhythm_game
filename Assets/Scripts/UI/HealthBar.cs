using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health")]
    public Slider slider;

    [Header("Container")]
    public Image containerImage;
    public WeaponController weaponController;
    public Sprite defaultContainerSprite;
    public GroupFillSprite[] groupFillSprites;

    void OnEnable()  => WeaponController.OnWeaponSwitched += OnWeaponSwitched;
    void OnDisable() => WeaponController.OnWeaponSwitched -= OnWeaponSwitched;

    public void SetMaxHealth(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value    = maxHealth;
    }

    public void SetHealth(float health)
    {
        slider.value = health;
    }

    void OnWeaponSwitched(int index)
    {
        if (containerImage == null || weaponController == null) return;
        if (index < 0 || index >= weaponController.EquippedWeapons.Count) return;

        Weapon active = weaponController.EquippedWeapons[index];

        if (active == weaponController.firstWeapon)
        {
            containerImage.sprite = defaultContainerSprite;
            return;
        }

        foreach (var group in weaponController.colourGroups)
        {
            if (group.weapons == null) continue;
            foreach (var w in group.weapons)
            {
                if (w.weapon != active) continue;

                foreach (var entry in groupFillSprites)
                {
                    if (entry.groupId == group.groupId)
                    {
                        containerImage.sprite = entry.sprite;
                        return;
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class GroupFillSprite
{
    public string groupId;
    public Sprite sprite;
}
