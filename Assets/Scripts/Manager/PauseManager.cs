using UnityEngine;

/// <summary>
/// Single source of truth for game-pause state.
/// Handles ESC input, FMOD fade, and timeScale.
/// Other systems (UpgradeUI, WeaponSelectUI) manage their own timeScale=0 windows
/// and are treated as "blocking" menus that prevent pause from opening.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public static bool IsPaused { get; private set; }

    [Header("FMOD")]
    [Tooltip("Seconds for the 'Pause Menu' parameter to fade in/out.")]
    public float fmodFadeDuration = 0.3f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else if (!IsBlockingMenuOpen())
            Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        BeatConductor.Instance.FadeParameter("Pause Menu", 1f, fmodFadeDuration);
        PauseMenuUI.Instance.Show();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        BeatConductor.Instance.FadeParameter("Pause Menu", 0f, fmodFadeDuration);
        PauseMenuUI.Instance.Hide();
    }

    // Returns true when upgrade/weapon-select menus own the timeScale=0 window.
    bool IsBlockingMenuOpen()
    {
        if (UpgradeUI.Instance != null && UpgradeUI.Instance.panel.activeSelf) return true;
        if (WeaponSelectUI.Instance != null && WeaponSelectUI.Instance.panel.activeSelf) return true;
        if (PassiveSelectionUI.Instance != null && PassiveSelectionUI.Instance.panel.activeSelf) return true;
        return false;
    }
}
