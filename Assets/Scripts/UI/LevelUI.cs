using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUI : MonoBehaviour
{
    public LevelSystem levelSystem;

    public TextMeshProUGUI levelText;
    public Slider expBar;

    void Update()
    {
        levelText.text = "Lv " + levelSystem.level;

        expBar.maxValue = levelSystem.expToNextLevel;
        expBar.value = levelSystem.currentExp;
    }
}