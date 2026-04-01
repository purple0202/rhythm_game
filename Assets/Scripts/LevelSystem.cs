using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public int level = 1;
    public float currentExp = 0;

    public float expToNextLevel = 100f;

    public void AddExp(float amount)
    {
        currentExp += amount;

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;

        expToNextLevel *= 1.2f;

        PlayerStats.Instance.ApplyLevelUp(level);
    }
}