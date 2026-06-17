using UnityEngine;
using System.Collections.Generic;

public class WeaponUI : MonoBehaviour
{
    public WeaponSlot[] weaponSlots;

    [Header("Group Lookup")]
    public WeaponController weaponController;
    public string firstWeaponGroupId = "Red";

    void Start()
    {
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
            string groupId = hasWeapon ? GetGroupId(equippedWeapons[i], i) : "";

            weaponSlots[i].SetWeapon(
                hasWeapon ? equippedWeapons[i].icon : null,
                hasWeapon ? equippedWeapons[i].weaponType : EnemyType.None,
                groupId
            );

            bool isPassive = hasWeapon && equippedWeapons[i].IsPassive;
            weaponSlots[i].SetSelected(hasWeapon && (i == selectedIndex || isPassive));
        }
    }

    string GetGroupId(Weapon weapon, int slotIndex)
    {
        if (slotIndex == 0) return firstWeaponGroupId;

        if (weaponController == null || weaponController.colourGroups == null) return "";

        foreach (var group in weaponController.colourGroups)
        {
            if (group.weapons == null) continue;
            foreach (var w in group.weapons)
                if (w.weapon == weapon) return group.groupId;
        }

        return "";
    }
}
