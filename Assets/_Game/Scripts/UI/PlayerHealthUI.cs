using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthController playerHealth;

    [SerializeField] private Image hpFill;

    [SerializeField] private TMP_Text hpText;

    private void Update()
    {
        if (playerHealth == null)
            return;

        float current =
            playerHealth.GetCurrentHP();

        float max =
            playerHealth.GetMaxHP();

        float percent =
            playerHealth.GetHealthPercent();

        if (hpFill != null)
            hpFill.fillAmount = percent;

        if (hpText != null)
            hpText.text =
                $"HP {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }
}