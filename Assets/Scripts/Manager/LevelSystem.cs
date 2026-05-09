using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public int level = 1;
    public float currentExp = 0;

    public float expToNextLevel = 100f;

    public bool xpBlocked = false;

    public void AddExp(float amount)
    {
        if (xpBlocked) return;
        currentExp += amount;

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    //void LevelUp()
    //{
    //    currentExp -= expToNextLevel;
    //    level++;

    //    expToNextLevel *= 1.2f;

    //    PlayerStats.Instance.ApplyLevelUp(level);
    //}

    public static event System.Action OnLevelUp;

    void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;

        expToNextLevel *= 1.2f;

        OnLevelUp?.Invoke();

        Time.timeScale = 0f;

        UpgradeUI.Instance.Show();
    }
}