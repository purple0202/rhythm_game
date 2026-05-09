using System.Collections.Generic;
using UnityEngine;

public class PassiveHUD : MonoBehaviour
{
    public GameObject iconPrefab;
    public Transform iconContainer;

    readonly Dictionary<PassiveData, PassiveIconUI> icons = new();

    void OnEnable()
    {
        PassiveManager.OnPassiveEquipped        += OnPassiveEquipped;
        PassiveEffect.OnActiveStateChanged      += OnActiveStateChanged;
        PassiveEffect.OnStackCountChanged       += OnStackCountChanged;
    }

    void OnDisable()
    {
        PassiveManager.OnPassiveEquipped        -= OnPassiveEquipped;
        PassiveEffect.OnActiveStateChanged      -= OnActiveStateChanged;
        PassiveEffect.OnStackCountChanged       -= OnStackCountChanged;
    }

    void OnPassiveEquipped(PassiveData data)
    {
        GameObject go = Instantiate(iconPrefab, iconContainer);
        PassiveIconUI iconUI = go.GetComponent<PassiveIconUI>();
        iconUI.Setup(data);
        icons[data] = iconUI;
    }

    void OnActiveStateChanged(PassiveData data, bool active)
    {
        if (icons.TryGetValue(data, out var icon))
            icon.SetActive(active);
    }

    void OnStackCountChanged(PassiveData data, string text)
    {
        if (icons.TryGetValue(data, out var icon))
            icon.SetStackCount(text);
    }
}
