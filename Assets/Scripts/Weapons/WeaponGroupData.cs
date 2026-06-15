using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Group Data")]
public class WeaponGroupData : WeaponBoxData
{
    public string groupId;
    public override void OnPickup(WeaponController controller) => controller.OpenGroupSelect(groupId);
}
