using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private HealthController targetHealth;
    [SerializeField] private UnitStats targetStats;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private string displayName = "Unit";

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    public void SetTarget(HealthController health, UnitStats stats, string unitDisplayName)
    {
        targetHealth = health;
        targetStats = stats;
        displayName = unitDisplayName;
        Refresh();
    }

    private void Refresh()
    {
        if (targetHealth == null || targetStats == null || hpSlider == null || labelText == null)
            return;

        float current = targetHealth.GetCurrentHP();
        float max = targetHealth.GetMaxHP();

        hpSlider.minValue = 0f;
        hpSlider.maxValue = max;
        hpSlider.value = current;

        labelText.text = $"{displayName} {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }
}