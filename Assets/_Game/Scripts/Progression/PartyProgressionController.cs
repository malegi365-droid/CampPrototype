using UnityEngine;

public class PartyProgressionController : MonoBehaviour
{
    [Header("Shared XP")]
    public int currentXP = 0;
    public int currentLevel = 1;

    [Header("XP Thresholds")]
    public int xpToLevel2 = 60;
    public int xpToLevel3 = 90;
    public int xpToLevel4 = 130;
    public int xpToLevel5 = 180;

    [Header("Party Members")]
    [SerializeField] private UnitStats dpsStats;
    [SerializeField] private AbilityController dpsAbilities;

    [SerializeField] private UnitStats tankStats;
    [SerializeField] private AbilityController tankAbilities;

    [SerializeField] private UnitStats healerStats;
    [SerializeField] private AbilityController healerAbilities;

    public void GainXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"Party gained {amount} XP. Total XP: {currentXP}");
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (currentLevel == 1 && currentXP >= xpToLevel2)
        {
            LevelUpTo2();
        }
        else if (currentLevel == 2 && currentXP >= xpToLevel2 + xpToLevel3)
        {
            LevelUpTo3();
        }
        else if (currentLevel == 3 && currentXP >= xpToLevel2 + xpToLevel3 + xpToLevel4)
        {
            LevelUpTo4();
        }
        else if (currentLevel == 4 && currentXP >= xpToLevel2 + xpToLevel3 + xpToLevel4 + xpToLevel5)
        {
            LevelUpTo5();
        }
    }

    private void LevelUpTo2()
    {
        currentLevel = 2;
        ApplyLevelToParty(2);
        Debug.Log("Party leveled to 2");
    }

    private void LevelUpTo3()
    {
        currentLevel = 3;
        ApplyLevelToParty(3);

        if (dpsAbilities != null)
        {
            dpsAbilities.UnlockFocusBreak();
        }

        Debug.Log("Party leveled to 3");
    }

    private void LevelUpTo4()
    {
        currentLevel = 4;
        ApplyLevelToParty(4);
        Debug.Log("Party leveled to 4");
    }

    private void LevelUpTo5()
    {
        currentLevel = 5;
        ApplyLevelToParty(5);
        Debug.Log("Party leveled to 5");
    }

    private void ApplyLevelToParty(int level)
    {
        if (dpsStats != null)
        {
            switch (level)
            {
                case 2: dpsStats.SetLevelStats(2, 110f, 14f, 5f); break;
                case 3: dpsStats.SetLevelStats(3, 122f, 16f, 6f); break;
                case 4: dpsStats.SetLevelStats(4, 133f, 18f, 7f); break;
                case 5: dpsStats.SetLevelStats(5, 145f, 21f, 8f); break;
            }
        }

        if (tankStats != null)
        {
            switch (level)
            {
                case 2: tankStats.SetLevelStats(2, 154f, 7f, 9f, 0f, 1.0f); break;
                case 3: tankStats.SetLevelStats(3, 169f, 8f, 10f, 0f, 1.0f); break;
                case 4: tankStats.SetLevelStats(4, 185f, 9f, 11f, 0f, 1.0f); break;
                case 5: tankStats.SetLevelStats(5, 202f, 10f, 12f, 0f, 1.0f); break;
            }
        }

        if (healerStats != null)
        {
            switch (level)
            {
                case 2: healerStats.SetLevelStats(2, 87f, 4f, 3f, 12f, 0.8f); break;
                case 3: healerStats.SetLevelStats(3, 95f, 5f, 4f, 14f, 0.8f); break;
                case 4: healerStats.SetLevelStats(4, 103f, 5f, 4f, 16f, 0.8f); break;
                case 5: healerStats.SetLevelStats(5, 112f, 6f, 5f, 18f, 0.8f); break;
            }
        }
    }

    public int GetCurrentXP()
    {
        return currentXP;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetPreviousThreshold(int level)
    {
        switch (level)
        {
            case 1: return 0;
            case 2: return xpToLevel2;
            case 3: return xpToLevel2 + xpToLevel3;
            case 4: return xpToLevel2 + xpToLevel3 + xpToLevel4;
            case 5: return xpToLevel2 + xpToLevel3 + xpToLevel4 + xpToLevel5;
            default: return 0;
        }
    }

    public int GetNextThreshold(int level)
    {
        switch (level)
        {
            case 1: return xpToLevel2;
            case 2: return xpToLevel2 + xpToLevel3;
            case 3: return xpToLevel2 + xpToLevel3 + xpToLevel4;
            case 4: return xpToLevel2 + xpToLevel3 + xpToLevel4 + xpToLevel5;
            default: return GetPreviousThreshold(level);
        }
    }
}