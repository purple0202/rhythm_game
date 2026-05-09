using UnityEngine;

public class InputJudge : MonoBehaviour
{
    public static event System.Action<string> OnJudgement;
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
        if (Time.timeScale == 0f) return;
        if (Input.GetKeyDown(KeyCode.P) || Input.GetMouseButtonDown(0))
            CheckInput();
    }

    void CheckInput()
    {
        float spb = BeatConductor.Instance.secondsPerBeat;
        if (spb <= 0) return;

        float beatPhase = Mathf.Repeat(BeatConductor.Instance.songPosition - BeatConductor.Instance.lastBeatTime - calibrationOffset, spb);
        float closest = Mathf.Min(beatPhase, spb - beatPhase);

        string judgement;
        if (closest <= perfectWindow)
            judgement = "Perfect";
        else if (closest <= greatWindow)
            judgement = "Good";
        else
            judgement = "Bad";

        Debug.Log($"[InputJudge] {judgement} | diff: {closest * 1000f:F1}ms | spb: {spb * 1000f:F1}ms");

        // Fire attack immediately on input — OnJudgement before PerformAttack so
        // passives that set PendingAttackBonus (e.g. JohnCageSilence) do so in time.
        judgementManager.ShowJudgement(judgement);
        OnJudgement?.Invoke(judgement);
        weaponController.PerformAttack(judgement);

        // Mark as consumed so OnBeat skips it
        pendingJudgement = "Consumed";
    }

    void OnBeat()
    {
        if (pendingJudgement == "Consumed")
        {
            pendingJudgement = "Auto";
            return;
        }

        // Auto — no input was made this beat
        judgementManager.ShowJudgement(pendingJudgement);
        OnJudgement?.Invoke(pendingJudgement);
        weaponController.PerformAttack(pendingJudgement);
        pendingJudgement = "Auto";
    }
}
