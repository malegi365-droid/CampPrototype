using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetUIController : MonoBehaviour
{
    [SerializeField] private TargetingController playerTargeting;
    [SerializeField] private Slider targetHPSlider;
    [SerializeField] private TextMeshProUGUI targetNameText;

    private void Update()
    {
        if (playerTargeting == null || targetHPSlider == null || targetNameText == null)
            return;

        Transform target = playerTargeting.GetCurrentTarget();

        if (target == null)
        {
            targetNameText.text = "No Target";
            targetHPSlider.minValue = 0f;
            targetHPSlider.maxValue = 1f;
            targetHPSlider.value = 0f;
            return;
        }

        HealthController health = target.GetComponent<HealthController>();
        UnitStats stats = target.GetComponent<UnitStats>();

        if (health == null || stats == null || health.IsDead())
        {
            targetNameText.text = "No Target";
            targetHPSlider.minValue = 0f;
            targetHPSlider.maxValue = 1f;
            targetHPSlider.value = 0f;
            return;
        }

        float current = health.GetCurrentHP();
        float max = health.GetMaxHP();

        targetNameText.text = $"{stats.unitName} HP: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        targetHPSlider.minValue = 0f;
        targetHPSlider.maxValue = max;
        targetHPSlider.value = current;
    }
}