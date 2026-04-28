using UnityEngine;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    public static event System.Action OnFirstWeaponEquipped;

    public WeaponUI weaponUI;

    [Header("First Weapon (fixed)")]
    public Weapon firstWeapon;

    [Header("Group Weapons (4 options each)")]
    public Weapon[] group2Weapons;
    public Weapon[] group3Weapons;
    public Weapon[] group4Weapons;

    private List<Weapon> equippedWeapons = new List<Weapon>();
    private int currentWeaponIndex = -1;

    void Awake()
    {
        DisableAll(firstWeapon);
        DisableGroup(group2Weapons);
        DisableGroup(group3Weapons);
        DisableGroup(group4Weapons);
    }

    void DisableAll(Weapon w) { if (w != null) w.gameObject.SetActive(false); }
    void DisableGroup(Weapon[] group) { if (group == null) return; foreach (var w in group) DisableAll(w); }

    void Update()
    {
        if (equippedWeapons.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && equippedWeapons.Count >= 1) SelectWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && equippedWeapons.Count >= 2) SelectWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && equippedWeapons.Count >= 3) SelectWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4) && equippedWeapons.Count >= 4) SelectWeapon(3);
    }

    // Called by WeaponBox on pickup
    public void OpenWeaponSelect()
    {
        int nextSlot = equippedWeapons.Count;

        if (nextSlot == 0)
        {
            EquipFirstWeapon();
            return;
        }

        Weapon[] options = nextSlot switch
        {
            1 => group2Weapons,
            2 => group3Weapons,
            3 => group4Weapons,
            _ => null
        };

        string fmodParam = nextSlot switch
        {
            1 => "Group 2",
            2 => "Group 3",
            3 => "Group 4",
            _ => ""
        };

        if (options == null || string.IsNullOrEmpty(fmodParam)) return;

        WeaponSelectUI.Instance.Show(options, fmodParam, this);
    }

    // Called by WeaponSelectUI after the player picks an option
    public void EquipGroupWeapon(Weapon weapon, string fmodParam, int fmodValue)
    {
        int slotIndex = equippedWeapons.Count;
        equippedWeapons.Add(weapon);

        if (slotIndex == 1)
            BeatConductor.Instance.SetParameter("Weapon 1", 1f);

        BeatConductor.Instance.SetParameter(fmodParam, (float)fmodValue);

        weaponUI?.RefreshUI(equippedWeapons, currentWeaponIndex);
    }

    public void PerformAttack(string judgement)
    {
        if (currentWeaponIndex < 0 || currentWeaponIndex >= equippedWeapons.Count) return;
        equippedWeapons[currentWeaponIndex].PerformAttack(judgement);
    }

    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= equippedWeapons.Count) return;

        if (currentWeaponIndex >= 0 && currentWeaponIndex < equippedWeapons.Count)
            equippedWeapons[currentWeaponIndex].gameObject.SetActive(false);

        currentWeaponIndex = index;
        equippedWeapons[currentWeaponIndex].gameObject.SetActive(true);

        weaponUI?.RefreshUI(equippedWeapons, currentWeaponIndex);
    }

    void EquipFirstWeapon()
    {
        if (firstWeapon == null) return;

        equippedWeapons.Add(firstWeapon);
        SelectWeapon(0);
        OnFirstWeaponEquipped?.Invoke();

        BeatConductor.Instance.SetParameter("Tutorial Success", 1f);
        BeatConductor.Instance.SetParameter("Synth 2", 1f);
        BeatConductor.Instance.SetParameter("Synth 3", 1f);
        BeatConductor.Instance.SetParameter("Synth 4", 1f);

        weaponUI?.RefreshUI(equippedWeapons, currentWeaponIndex);
    }
}
