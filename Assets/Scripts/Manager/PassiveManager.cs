using System.Collections.Generic;
using UnityEngine;

public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Instance { get; private set; }

    [System.Serializable]
    public struct PassiveBinding
    {
        public PassiveData data;
        public PassiveEffect effect;
    }

    public PassiveBinding[] bindings;
    public int maxPassives = 3;
    public int optionsToShow = 3;

    public static event System.Action<PassiveData> OnPassiveEquipped;

    readonly List<PassiveData> equippedPassives = new();

    void Awake() => Instance = this;

    public int EquippedCount => equippedPassives.Count;
    public bool IsFull => equippedPassives.Count >= maxPassives;
    public bool IsEquipped(PassiveData data) => equippedPassives.Contains(data);

    public void OpenSelection()
    {
        if (IsFull) return;
        PassiveData[] options = GetRandomOptions();
        if (options.Length == 0) return;
        PassiveSelectionUI.Instance.Show(options);
    }

    public void Equip(PassiveData data)
    {
        if (IsEquipped(data)) return;

        foreach (var b in bindings)
        {
            if (b.data != data) continue;
            equippedPassives.Add(data);
            b.effect.enabled = true;
            b.effect.OnActivate();
            OnPassiveEquipped?.Invoke(data);
            return;
        }
    }

    public float GetDamageMultiplier()
    {
        float multiplier = 1f;
        foreach (var b in bindings)
            if (IsEquipped(b.data))
                multiplier *= b.effect.GetDamageMultiplier();
        return multiplier;
    }

    PassiveData[] GetRandomOptions()
    {
        var available = new List<PassiveData>();
        foreach (var b in bindings)
            if (!IsEquipped(b.data)) available.Add(b.data);

        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        int count = Mathf.Min(optionsToShow, available.Count);
        var result = new PassiveData[count];
        for (int i = 0; i < count; i++) result[i] = available[i];
        return result;
    }
}
