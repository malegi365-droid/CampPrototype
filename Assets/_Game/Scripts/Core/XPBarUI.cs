using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    [SerializeField] private PartyProgressionController partyProgression;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Update()
    {
        if (partyProgression == null || xpSlider == null || levelText == null)
            return;

        int currentLevel = partyProgression.GetCurrentLevel();
        int currentXP = partyProgression.GetCurrentXP();

        int previousThreshold = 0;
        if (currentLevel > 1)
        {
            previousThreshold = partyProgression.GetPreviousThreshold(currentLevel - 1);
        }

        int nextThreshold = partyProgression.GetNextThreshold(currentLevel);

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
}