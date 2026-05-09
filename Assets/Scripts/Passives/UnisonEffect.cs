using UnityEngine;

public class UnisonEffect : PassiveEffect
{
    [Header("Settings")]
    public int attacksPerUnison = 4;

    [Header("References")]
    public WeaponController weaponController;

    int counter;
    bool isActive;

    public override void OnActivate()
    {
        counter = 0;
        isActive = false;
        InputJudge.OnJudgement += OnJudgement;
        NotifyActiveState(false);
    }

    void OnDisable()
    {
        InputJudge.OnJudgement -= OnJudgement;
    }

    void OnJudgement(string judgement)
    {
        if (judgement == "Auto")
        {
            if (Time.timeScale == 0f) return;
            if (isActive)
            {
                counter = 0;
                isActive = false;
                NotifyActiveState(false);
                NotifyStackCount("");
            }
            return;
        }

        if (judgement == "Bad") return;

        // Perfect or Good — count the hit
        counter++;

        if (!isActive)
        {
            isActive = true;
            NotifyActiveState(true);
        }

        NotifyStackCount($"{counter}");

        if (counter >= attacksPerUnison)
        {
            weaponController.PerformUnisonAttack(judgement);
            counter = 0;
            // Icon stays lit — only dims on the next missed beat
        }
    }
}
