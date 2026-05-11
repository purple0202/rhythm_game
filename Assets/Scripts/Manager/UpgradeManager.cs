using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [System.Serializable]
    public struct UpgradeBinding
    {
        public UpgradeData   data;
        public UpgradeEffect effect;
    }

    public UpgradeBinding[] bindings;
    public int optionsToShow  = 4;
    public int totalRerolls   = 0;
    public int rerollsAvailable = 0;

    public void ResetRerolls() => rerollsAvailable = totalRerolls;

    readonly Dictionary<UpgradeData, int> appliedCounts = new();

    void Awake()
    {
        Instance = this;
        // Let effects know which data asset they belong to so IsStackable can call GetAppliedCount.
        foreach (var b in bindings)
            if (b.effect != null) b.effect.data = b.data;
    }

    public void Apply(UpgradeData data)
    {
        foreach (var b in bindings)
        {
            if (b.data != data) continue;
            b.effect.Apply();
            appliedCounts[data] = GetAppliedCount(data) + 1;
            return;
        }
    }

    public int GetAppliedCount(UpgradeData data) =>
        appliedCounts.TryGetValue(data, out int c) ? c : 0;

    public List<UpgradeData> GetRandomOptions()
    {
        var available = new List<UpgradeData>();
        foreach (var b in bindings)
        {
            if (!b.data.enabled) continue;
            if (b.effect.IsStackable || GetAppliedCount(b.data) == 0)
                available.Add(b.data);
        }

        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        int count = Mathf.Min(optionsToShow, available.Count);
        return available.GetRange(0, count);
    }
}
