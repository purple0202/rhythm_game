using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Drop Data")]
public class WeaponDropData : WeaponBoxData
{
    public override void OnPickup(WeaponController controller) => controller.EquipDirect();
}
