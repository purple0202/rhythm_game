using UnityEngine;

public abstract class PassiveEffect : MonoBehaviour
{
    [HideInInspector] public PassiveData data;

    public virtual void OnActivate() { }
    public virtual float GetDamageMultiplier() => 1f;

    public static event System.Action<PassiveData, bool> OnActiveStateChanged;
    public static event System.Action<PassiveData, string> OnStackCountChanged;

    protected void NotifyActiveState(bool active) => OnActiveStateChanged?.Invoke(data, active);
    protected void NotifyStackCount(string text)  => OnStackCountChanged?.Invoke(data, text);
}
