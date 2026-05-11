using UnityEngine;

// One subclass per upgrade. Place all UpgradeEffect components on the UpgradeManager GameObject.
public abstract class UpgradeEffect : MonoBehaviour
{
    [HideInInspector] public UpgradeData data;

    // False = only appears once; True = can be offered again on future level-ups.
    public virtual bool IsStackable => true;

    public abstract void Apply();
}
