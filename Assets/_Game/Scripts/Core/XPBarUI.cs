using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    [SerializeField] private XPController playerXP;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Update()
    {
        if (playerXP == null || xpSlider == null || levelText == null)
            return;

        int currentLevel = playerXP.currentLevel;
        int currentXP = playerXP.currentXP;

        int previousThreshold = GetPreviousThreshold(currentLevel);
        int nextThreshold = GetNextThreshold(currentLevel);

        int xpIntoLevel = currentXP - previousThreshold;
        int xpNeededThisLevel = nextThreshold - previousThreshold;

        if (xpNeededThisLevel <= 0)
        {
            xpSlider.minValue = 0;
            xpSlider.maxValue = 1;
            xpSlider.value = 1;
        }
        else
        {
            xpSlider.minValue = 0;
            xpSlider.maxValue = xpNeededThisLevel;
            xpSlider.value = xpIntoLevel;
        }

        levelText.text = $"Level {currentLevel} | XP: {currentXP}";
    }

    private int GetPreviousThreshold(int level)
    {
        switch (level)
        {
            case 1: return 0;
            case 2: return playerXP.xpToLevel2;
            case 3: return playerXP.xpToLevel2 + playerXP.xpToLevel3;
            case 4: return playerXP.xpToLevel2 + playerXP.xpToLevel3 + playerXP.xpToLevel4;
            case 5: return playerXP.xpToLevel2 + playerXP.xpToLevel3 + playerXP.xpToLevel4 + playerXP.xpToLevel5;
            default: return 0;
        }
    }

    private int GetNextThreshold(int level)
    {
        switch (level)
        {
            case 1: return playerXP.xpToLevel2;
            case 2: return playerXP.xpToLevel2 + playerXP.xpToLevel3;
            case 3: return playerXP.xpToLevel2 + playerXP.xpToLevel3 + playerXP.xpToLevel4;
            case 4: return playerXP.xpToLevel2 + playerXP.xpToLevel3 + playerXP.xpToLevel4 + playerXP.xpToLevel5;
            default: return GetPreviousThreshold(level);
        }
    }
}