using UnityEngine;

[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(AbilityController))]
public class XPController : MonoBehaviour
{
    private UnitStats stats;
    private AbilityController abilityController;

    [Header("XP")]
    public int currentXP = 0;
    public int currentLevel = 1;

    [Header("XP Thresholds")]
    public int xpToLevel2 = 60;
    public int xpToLevel3 = 90;
    public int xpToLevel4 = 130;
    public int xpToLevel5 = 180;

    [Header("Party Members")]
    [SerializeField] private UnitStats tankStats;
    [SerializeField] private UnitStats healerStats;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
        abilityController = GetComponent<AbilityController>();
        currentLevel = stats.level;
    }

    public void GainXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"{gameObject.name} gained {amount} XP. Total XP: {currentXP}");
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
        stats.SetLevelStats(2, 110f, 14f, 5f);
        ApplyPartyLevel(2);
        Debug.Log("Leveled up to 2");
    }

    private void LevelUpTo3()
    {
        currentLevel = 3;
        stats.SetLevelStats(3, 122f, 16f, 6f);
        abilityController.UnlockFocusBreak();
        ApplyPartyLevel(3);
        Debug.Log("Leveled up to 3 and unlocked Focus Break");
    }

    private void LevelUpTo4()
    {
        currentLevel = 4;
        stats.SetLevelStats(4, 133f, 18f, 7f);
        ApplyPartyLevel(4);
        Debug.Log("Leveled up to 4");
    }

    private void LevelUpTo5()
    {
        currentLevel = 5;
        stats.SetLevelStats(5, 145f, 21f, 8f);
        ApplyPartyLevel(5);
        Debug.Log("Leveled up to 5");
    }

    private void ApplyPartyLevel(int level)
    {
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
}