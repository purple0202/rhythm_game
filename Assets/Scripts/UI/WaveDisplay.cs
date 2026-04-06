using UnityEngine;
using UnityEngine.UI;

public class WaveDisplay : MonoBehaviour
{
    public WaveManager waveManager;

    public Text waveText;
    //public Slider expBar;

    void Update()
    {
        waveText.text = "WAVE " + (waveManager.currentWaveIndex+1);

        //expBar.maxValue = levelSystem.expToNextLevel;
        //expBar.value = levelSystem.currentExp;
    }
}