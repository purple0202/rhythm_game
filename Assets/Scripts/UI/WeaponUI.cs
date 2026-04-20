using UnityEngine;
using System.Collections.Generic;

public class WeaponUI : MonoBehaviour
{
    public WeaponSlot[] weaponSlots;

    void Start()
    {
        // Show all slots as empty on startup
        foreach (var slot in weaponSlots)
        {
            slot.gameObject.SetActive(true);
            slot.SetWeapon(null);
        }
    }

    public void RefreshUI(List<Weapon> equippedWeapons, int selectedIndex)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            bool hasWeapon = i < equippedWeapons.Count;
            weaponSlots[i].SetWeapon(
                hasWeapon ? equippedWeapons[i].icon : null,
                hasWeapon ? equippedWeapons[i].weaponType : EnemyType.None
            );
            weaponSlots[i].SetSelected(hasWeapon && i == selectedIndex);
        }
    }
}
