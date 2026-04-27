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

    void Awake()
    {
        Weapon[] allWeapons = { FirstWeapon, SecondWeapon, ThirdWeapon, FourthWeapon };
        foreach (var w in allWeapons)
            if (w != null) w.gameObject.SetActive(false);
    }

    void Update()
    {
        if (equippedWeapons.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && equippedWeapons.Count >= 1) SelectWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && equippedWeapons.Count >= 2) SelectWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && equippedWeapons.Count >= 3) SelectWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4) && equippedWeapons.Count >= 4) SelectWeapon(3);
    }

    public void EquipWeapon()
    {
        Weapon[] allWeapons = { FirstWeapon, SecondWeapon, ThirdWeapon, FourthWeapon };
        int nextIndex = equippedWeapons.Count;

        if (nextIndex >= allWeapons.Length) return;

        Weapon toEquip = allWeapons[nextIndex];
        if (toEquip == null) return;

        bool isFirst = nextIndex == 0;
        equippedWeapons.Add(toEquip);

        if (isFirst)
        {
            SelectWeapon(0);
            OnFirstWeaponEquipped?.Invoke();
            BeatConductor.Instance.SetParameter("Tutorial Success", 1f);
            BeatConductor.Instance.SetParameter("Synth 2", 1f);
            BeatConductor.Instance.SetParameter("Synth 3", 1f);
            BeatConductor.Instance.SetParameter("Synth 4", 1f);
        }
        else if (nextIndex == 1)
        {
            BeatConductor.Instance.SetParameter("Weapon 1", 1f);
            BeatConductor.Instance.SetParameter("Group 2", 1f);
        }
        else if (nextIndex == 2)
        {
            BeatConductor.Instance.SetParameter("Group 3", 1f);
        }
        else if (nextIndex == 3)
        {
            BeatConductor.Instance.SetParameter("Group 4", 1f);
        }

        if (weaponUI != null)
            weaponUI.RefreshUI(equippedWeapons, currentWeaponIndex);
    }

    public void PerformAttack(string judgement)
    {
        if (currentWeaponIndex < 0 || currentWeaponIndex >= equippedWeapons.Count) return;
        equippedWeapons[currentWeaponIndex].PerformAttack(judgement);
    }

    void SelectWeapon(int index)
    {
        if (index < 0 || index >= equippedWeapons.Count) return;
        Debug.Log("reached!!");
        if (currentWeaponIndex >= 0 && currentWeaponIndex < equippedWeapons.Count)
            equippedWeapons[currentWeaponIndex].gameObject.SetActive(false);

        currentWeaponIndex = index;
        equippedWeapons[currentWeaponIndex].gameObject.SetActive(true);

        if (weaponUI != null)
            weaponUI.RefreshUI(equippedWeapons, currentWeaponIndex);
    }
}
