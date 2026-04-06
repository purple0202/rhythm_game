using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    public LevelSystem levelSystem;

    public Text levelText;
    public Slider expBar;

    void Update()
    {
        levelText.text = "Lv " + levelSystem.level;

        expBar.maxValue = levelSystem.expToNextLevel;
        expBar.value = levelSystem.currentExp;
    }
}