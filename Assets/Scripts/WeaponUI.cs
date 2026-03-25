using UnityEngine;

public class WeaponUI : MonoBehaviour
{
    public WeaponSlot[] weaponSlots;

    public void UpdateUI(int selectedIndex)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].SetSelected(i == selectedIndex);
        }
    }
}