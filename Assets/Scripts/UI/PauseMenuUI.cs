using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the pause menu panel.
/// MainView shows the top-level options; sub-panels (e.g. CalibrationUI) swap in/out
/// while MainView is hidden. Add new sub-panels by calling ShowSubPanel() from a button.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance;

    [Header("Root Panel")]
    public GameObject pausePanel;

    [Header("Main View")]
    public GameObject mainView;
    public Button resumeButton;
    public Button calibrationButton;
    public Toggle damageDisplayToggle;

    [Header("Sub Panels")]
    public CalibrationUI calibrationUI;

    void Awake()
    {
        Instance = this;
        pausePanel.SetActive(false);
    }

    void Start()
    {
        resumeButton.onClick.AddListener(PauseManager.Instance.Resume);
        calibrationButton.onClick.AddListener(OpenCalibration);

        damageDisplayToggle.isOn = DamagePopup.IsVisible;
        damageDisplayToggle.onValueChanged.AddListener(DamagePopup.SetVisible);
    }

    public void Show()
    {
        pausePanel.SetActive(true);
        ShowMainView();
    }

    public void Hide()
    {
        pausePanel.SetActive(false);
    }

    void ShowMainView()
    {
        mainView.SetActive(true);
        calibrationUI.gameObject.SetActive(false);
    }

    void OpenCalibration()
    {
        mainView.SetActive(false);
        calibrationUI.gameObject.SetActive(true);
        calibrationUI.OnOpen();
    }

    // Called by sub-panel Back buttons to return to main view.
    public void ReturnToMainView()
    {
        ShowMainView();
    }
}
