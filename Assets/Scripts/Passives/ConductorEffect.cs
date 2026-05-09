using UnityEngine;

public class ConductorEffect : PassiveEffect
{
    InputJudge inputJudge;
    float originalPerfectWindow;
    float originalGreatWindow;

    public override void OnActivate()
    {
        inputJudge = FindObjectOfType<InputJudge>();

        originalPerfectWindow = inputJudge.perfectWindow;
        originalGreatWindow   = inputJudge.greatWindow;

        // Expand perfect window to cover the good zone, collapse good window
        inputJudge.perfectWindow = originalGreatWindow;
        inputJudge.greatWindow   = originalGreatWindow;

        NotifyActiveState(true);
    }

    void OnDisable()
    {
        if (inputJudge == null) return;
        inputJudge.perfectWindow = originalPerfectWindow;
        inputJudge.greatWindow   = originalGreatWindow;
    }

    // Stub: called by tempo-attack system when implemented
    public bool IsImmuneToTempoAttacks => true;
}
