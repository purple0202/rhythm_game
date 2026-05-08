using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance { get; private set; }

    [Tooltip("How many beats the player can miss before the combo breaks.")]
    public int forgiveness = 3;

    public static event System.Action<int> OnComboChanged;
    public static event System.Action       OnComboReset;

    private int comboCount;
    private int missedBeats;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()  => InputJudge.OnJudgement += HandleJudgement;
    void OnDisable() => InputJudge.OnJudgement -= HandleJudgement;

    void HandleJudgement(string judgement)
    {
        switch (judgement)
        {
            case "Perfect":
            case "Good":
                missedBeats = 0;
                comboCount++;
                OnComboChanged?.Invoke(comboCount);
                break;

            case "Bad":
                BreakCombo();
                break;

            case "Auto":
                missedBeats++;
                if (missedBeats > forgiveness)
                    BreakCombo();
                break;
        }
    }

    void BreakCombo()
    {
        if (comboCount == 0) return;
        comboCount  = 0;
        missedBeats = 0;
        OnComboReset?.Invoke();
        OnComboChanged?.Invoke(0);
    }

    public int GetCombo() => comboCount;
}
