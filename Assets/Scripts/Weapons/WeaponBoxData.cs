using UnityEngine;

public abstract class WeaponBoxData : ScriptableObject
{
    public RuntimeAnimatorController animatorController;
    public abstract void OnPickup(WeaponController controller);
}
