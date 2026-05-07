using UnityEngine;

public class InputJudge : MonoBehaviour
{
    public JudgementManager judgementManager;
    public WeaponController weaponController;

    public float perfectWindow = 0.05f;
    public float greatWindow = 0.1f;
    public float goodWindow = 0.15f;

    [Header("Calibration")]
    [Tooltip("Shift the beat window to compensate for audio output latency. Increase if hits feel early, decrease if they feel late. (seconds)")]
    public float calibrationOffset = 0f;

    private string pendingJudgement = "Auto";

    void Start()
    {
        // Load persisted calibration so it's applied from the very first beat.
        calibrationOffset = PlayerPrefs.GetFloat(CalibrationUI.PrefKey, 0f) / 1000f;
    }

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
            CheckInput();
    }

    void CheckInput()
    {
        float spb = BeatConductor.Instance.secondsPerBeat;
        if (spb <= 0) return;

        float beatPhase = Mathf.Repeat(BeatConductor.Instance.songPosition - BeatConductor.Instance.lastBeatTime - calibrationOffset, spb);
        float closest = Mathf.Min(beatPhase, spb - beatPhase);

        if (closest <= perfectWindow)
            pendingJudgement = "Perfect";
        else if (closest <= greatWindow)
            pendingJudgement = "Good";
        else
            pendingJudgement = "Bad";

        Debug.Log($"[InputJudge] {pendingJudgement} | diff: {closest * 1000f:F1}ms | spb: {spb * 1000f:F1}ms");
    }

    void OnBeat()
    {
        judgementManager.ShowJudgement(pendingJudgement);
        weaponController.PerformAttack(pendingJudgement);
        pendingJudgement = "Auto";
    }
}
