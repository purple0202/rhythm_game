using UnityEngine;
using TMPro;

public class WaveDisplay : MonoBehaviour
{
    public WaveManager waveManager;

    public TextMeshProUGUI waveText;
    //public Slider expBar;

    void Update()
    {
        waveText.text = "WAVE " + (waveManager.currentWaveIndex+1);

        //expBar.maxValue = levelSystem.expToNextLevel;
        //expBar.value = levelSystem.currentExp;
    }
}