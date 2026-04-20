using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AbilityController playerAbilities;

    [Header("Heavy Shot UI")]
    [SerializeField] private Image heavyShotBackground;
    [SerializeField] private TextMeshProUGUI heavyShotNameText;
    [SerializeField] private TextMeshProUGUI heavyShotStatusText;

    [Header("Focus Break UI")]
    [SerializeField] private Image focusBreakBackground;
    [SerializeField] private TextMeshProUGUI focusBreakNameText;
    [SerializeField] private TextMeshProUGUI focusBreakStatusText;

    [Header("Colors")]
    [SerializeField] private Color readyColor = new Color(0.2f, 0.6f, 0.2f, 0.8f);
    [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.2f, 0.8f);
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    private void Update()
    {
        if (playerAbilities == null) return;

        RefreshHeavyShot();
        RefreshFocusBreak();
    }

    private void RefreshHeavyShot()
    {
        if (heavyShotNameText != null)
            heavyShotNameText.text = "1 - Heavy Shot";

        bool unlocked = playerAbilities.HasHeavyShot();
        float remaining = playerAbilities.GetHeavyShotRemainingCooldown();

        if (!unlocked)
        {
            if (heavyShotStatusText != null) heavyShotStatusText.text = "Locked";
            if (heavyShotBackground != null) heavyShotBackground.color = lockedColor;
            return;
        }

        if (remaining > 0f)
        {
            if (heavyShotStatusText != null) heavyShotStatusText.text = $"CD: {remaining:F1}";
            if (heavyShotBackground != null) heavyShotBackground.color = cooldownColor;
        }
        else
        {
            if (heavyShotStatusText != null) heavyShotStatusText.text = "Ready";
            if (heavyShotBackground != null) heavyShotBackground.color = readyColor;
        }
    }

    private void RefreshFocusBreak()
    {
        if (focusBreakNameText != null)
            focusBreakNameText.text = "2 - Focus Break";

        bool unlocked = playerAbilities.HasFocusBreak();
        float remaining = playerAbilities.GetFocusBreakRemainingCooldown();

        if (!unlocked)
        {
            if (focusBreakStatusText != null) focusBreakStatusText.text = "Locked";
            if (focusBreakBackground != null) focusBreakBackground.color = lockedColor;
            return;
        }

        if (remaining > 0f)
        {
            if (focusBreakStatusText != null) focusBreakStatusText.text = $"CD: {remaining:F1}";
            if (focusBreakBackground != null) focusBreakBackground.color = cooldownColor;
        }
        else
        {
            if (focusBreakStatusText != null) focusBreakStatusText.text = "Ready";
            if (focusBreakBackground != null) focusBreakBackground.color = readyColor;
        }
    }
}