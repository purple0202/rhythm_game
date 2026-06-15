using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHUD : MonoBehaviour
{
    public static BossHUD Instance { get; private set; }

    [Header("References")]
    [SerializeField] GameObject hudPanel;
    [SerializeField] Image healthFill;
    [SerializeField] TMP_Text bossNameText;

    EnemyHealth bossHealth;
    bool loggedNullWarning;

    void Awake()
    {
        Instance = this;
        hudPanel.SetActive(false);
    }

    public void Show(string bossName, EnemyType type, int patternLength, EnemyHealth health)
    {
        bossHealth = health;
        loggedNullWarning = false;
        Debug.Log($"[BossHUD] Show called — health ref: {health}, maxHealth: {health?.maxHealth}, currentHealth: {health?.CurrentHealth}");

        if (type != EnemyType.None)
            healthFill.color = TypeToColor(type);
        bossNameText.text = bossName;

        hudPanel.SetActive(true);
    }

    public void Hide()
    {
        hudPanel.SetActive(false);
        bossHealth = null;
    }

    public void SetPattern(int patternLength) { }

    void Update()
    {
        if (bossHealth != null)
        {
            healthFill.fillAmount = bossHealth.CurrentHealth / bossHealth.maxHealth;
        }
        else if (!loggedNullWarning)
        {
            Debug.LogWarning("[BossHUD] bossHealth is null — Show() was not called or received a null EnemyHealth");
            loggedNullWarning = true;
        }
    }

    static Color TypeToColor(EnemyType t) => t switch
    {
        EnemyType.Blue   => new Color(0.3f, 0.7f, 1f),
        EnemyType.Red    => new Color(1f, 0.35f, 0.35f),
        EnemyType.Green  => new Color(0.3f, 1f, 0.45f),
        EnemyType.Yellow => new Color(1f, 0.9f, 0.2f),
        _                => Color.white,
    };
}
