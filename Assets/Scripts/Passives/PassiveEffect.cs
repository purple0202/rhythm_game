using UnityEngine;

public abstract class PassiveEffect : MonoBehaviour
{
    public virtual void OnActivate() { }
    public virtual float GetDamageMultiplier() => 1f;
}
