using UnityEngine;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    public static event System.Action OnFirstWeaponEquipped;

    public WeaponUI weaponUI;
    public Weapon FirstWeapon;
    public Weapon SecondWeapon;
    public Weapon ThirdWeapon;
    public Weapon FourthWeapon;

    private List<Weapon> equippedWeapons = new List<Weapon>();
    private int currentWeaponIndex = -1;

    void Update()
    {
        if (equippedWeapons.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && equippedWeapons.Count >= 1)
            SelectWeapon(0);

        if (Input.GetKeyDown(KeyCode.Alpha2) && equippedWeapons.Count >= 2)
            SelectWeapon(1);
    }

    public void EquipWeapon()
    {
        bool isFirst = equippedWeapons.Count == 0;


        // equippedWeapons.Add();
        // weapon.gameObject.SetActive(false);

        // Auto-select if this is the first weapon
        if (isFirst)
        {
            equippedWeapons.Add(FirstWeapon);
            SelectWeapon(0);
            OnFirstWeaponEquipped?.Invoke();
        } else if(equippedWeapons.Count ==1)
        //Case for adding second weapon + later implement third/fourth
        {
            equippedWeapons.Add(SecondWeapon);
        }

        if (weaponUI != null)
            weaponUI.RefreshUI(equippedWeapons, currentWeaponIndex);
    }

    // public void EquipWeapon(Weapon weapon)
    // {
    //     bool isFirst = equippedWeapons.Count == 0;

    //     equippedWeapons.Add(weapon);
    //     weapon.gameObject.SetActive(false);

    //     // Auto-select if this is the first weapon
    //     if (isFirst)
    //     {
    //         SelectWeapon(0);
    //         OnFirstWeaponEquipped?.Invoke();
    //     }

    //     if (weaponUI != null)
    //         weaponUI.RefreshUI(equippedWeapons, currentWeaponIndex);
    // }

    public void PerformAttack(string judgement)
    {
        if (currentWeaponIndex < 0 || currentWeaponIndex >= equippedWeapons.Count) return;
        equippedWeapons[currentWeaponIndex].PerformAttack(judgement);
    }

    void SelectWeapon(int index)
    {
        if (index < 0 || index >= equippedWeapons.Count) return;

        if (currentWeaponIndex >= 0 && currentWeaponIndex < equippedWeapons.Count)
            equippedWeapons[currentWeaponIndex].gameObject.SetActive(false);

        currentWeaponIndex = index;
        equippedWeapons[currentWeaponIndex].gameObject.SetActive(true);

        if (weaponUI != null)
            weaponUI.RefreshUI(equippedWeapons, currentWeaponIndex);
    }
}
