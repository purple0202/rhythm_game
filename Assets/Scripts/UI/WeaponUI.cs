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
            Debug.Log("COUNT: " + equippedWeapons.Count + hasWeapon);
            weaponSlots[i].SetWeapon(hasWeapon ? equippedWeapons[i].icon : null);
            weaponSlots[i].SetSelected(hasWeapon && i == selectedIndex);
        }
    }
}
