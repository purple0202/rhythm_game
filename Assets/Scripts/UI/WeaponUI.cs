using UnityEngine;
using System.Collections.Generic;

public class WeaponUI : MonoBehaviour
{
    public WeaponSlot[] weaponSlots;

    public void RefreshUI(List<Weapon> equippedWeapons, int selectedIndex)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            bool hasWeapon = i < equippedWeapons.Count;
            weaponSlots[i].gameObject.SetActive(hasWeapon);

            if (hasWeapon)
                weaponSlots[i].SetSelected(i == selectedIndex);
        }
    }
}
