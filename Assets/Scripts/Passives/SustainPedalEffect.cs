using UnityEngine;

public class SustainPedalEffect : PassiveEffect
{
    int originalForgiveness;

    public override void OnActivate()
    {
        originalForgiveness = ComboSystem.Instance.forgiveness;
        ComboSystem.Instance.forgiveness = originalForgiveness * 2;
        NotifyActiveState(true);
    }

    void OnDisable()
    {
        if (ComboSystem.Instance != null)
            ComboSystem.Instance.forgiveness = originalForgiveness;
    }
}
