using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sub-panel of the pause menu for adjusting audio calibration offset.
/// The offset is displayed in milliseconds and persisted with PlayerPrefs.
/// Positive offset = hits feel early (shift window later).
/// Negative offset = hits feel late (shift window earlier).
/// </summary>
public class CalibrationUI : MonoBehaviour
{
    [Header("References")]
    public InputJudge inputJudge;

    [Header("UI")]
    public TextMeshProUGUI offsetLabel;
    public Button increaseButton;
    public Button decreaseButton;
    public Button backButton;

    [Header("Step Size")]
    [Tooltip("How many milliseconds each +/- press adjusts the offset.")]
    public float stepMs = 5f;

    internal const string PrefKey = "AudioCalibrationOffsetMs";

    void Start()
    {
        increaseButton.onClick.AddListener(() => Adjust(stepMs));
        decreaseButton.onClick.AddListener(() => Adjust(-stepMs));
        backButton.onClick.AddListener(PauseMenuUI.Instance.ReturnToMainView);
    }

    // Called by PauseMenuUI whenever this panel becomes visible.
    public void OnOpen()
    {
        RefreshLabel();
    }

    void Adjust(float deltaMs)
    {
        float currentMs = inputJudge.calibrationOffset * 1000f;
        currentMs = Mathf.Clamp(currentMs + deltaMs, -200f, 200f);
        inputJudge.calibrationOffset = currentMs / 1000f;
        PlayerPrefs.SetFloat(PrefKey, currentMs);
        PlayerPrefs.Save();
        RefreshLabel();
    }

    void RefreshLabel()
    {
        float ms = inputJudge.calibrationOffset * 1000f;
        string sign = ms > 0f ? "+" : "";
        offsetLabel.text = $"Audio Offset: {sign}{ms:F0} ms";
    }
}
