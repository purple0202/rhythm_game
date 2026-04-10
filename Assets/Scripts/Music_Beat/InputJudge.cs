using UnityEngine;

public class InputJudge : MonoBehaviour
{
    public BeatmapData beatmap;
    public JudgementManager judgementManager;
    public WeaponController weaponController;

    public float perfectWindow = 0.05f;
    public float greatWindow = 0.1f;
    public float goodWindow = 0.15f;

    private string pendingJudgement = "Auto";

    void OnEnable()
    {
        BeatConductor.OnBeat += OnBeat;
    }

    void OnDisable()
    {
        BeatConductor.OnBeat -= OnBeat;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetMouseButtonDown(0))
        {
            CheckInput();
        }
    }

    void CheckInput()
    {
        float currentTime = BeatConductor.Instance.songPosition;
        float closest = float.MaxValue;

        foreach (float beat in beatmap.beatTimings)
        {
            float diff = Mathf.Abs(beat - currentTime);
            if (diff < closest)
                closest = diff;
        }

        if (closest <= perfectWindow)
            pendingJudgement = "Perfect";
        else if (closest <= greatWindow)
            pendingJudgement = "Good";
        else if (closest <= goodWindow)
            pendingJudgement = "Bad";
        else
            pendingJudgement = "Bad";
    }

    void OnBeat()
    {
        judgementManager.ShowJudgement(pendingJudgement);
        weaponController.PerformAttack(pendingJudgement);
        pendingJudgement = "Auto";
    }
}
