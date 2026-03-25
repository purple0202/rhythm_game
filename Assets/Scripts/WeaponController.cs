using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Weapon[] weapons;
    public WeaponUI weaponUI;

    int currentWeaponIndex = 0;

    //public WeaponUI weaponUI;

    void Start()
    {
        EquipWeapon(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquipWeapon(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            EquipWeapon(1);
    }

    public void PerformAttack(string judgement)
    {
        weapons[currentWeaponIndex].PerformAttack(judgement);
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length)
            return;

        currentWeaponIndex = index;

        if (weaponUI != null)
            weaponUI.UpdateUI(index);
        //weaponUI.SetWeapon(weapons[index].weaponName);
    }
}