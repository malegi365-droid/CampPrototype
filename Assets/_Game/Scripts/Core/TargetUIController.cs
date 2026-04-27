using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyControlManager partyControlManager;

    [Header("UI")]
    [SerializeField] private Slider targetHPSlider;
    [SerializeField] private TextMeshProUGUI targetNameText;

    private void Awake()
    {
        if (partyControlManager == null)
            partyControlManager = FindAnyObjectByType<PartyControlManager>();
    }

    private void Update()
    {
        TargetingController activeTargeting = GetActiveTargeting();

        if (activeTargeting == null || targetHPSlider == null || targetNameText == null)
            return;

        Transform target = activeTargeting.GetCurrentTarget();

        if (target == null)
        {
            ShowNoTarget();
            return;
        }

        HealthController health = target.GetComponent<HealthController>();
        UnitStats stats = target.GetComponent<UnitStats>();

        if (health == null || stats == null || health.IsDead())
        {
            ShowNoTarget();
            return;
        }

        float current = health.GetCurrentHP();
        float max = health.GetMaxHP();

        targetNameText.text = $"{stats.unitName} HP: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        targetHPSlider.minValue = 0f;
        targetHPSlider.maxValue = max;
        targetHPSlider.value = current;
    }

    private TargetingController GetActiveTargeting()
    {
        if (partyControlManager == null || partyControlManager.CurrentMember == null)
            return null;

        return partyControlManager.CurrentMember.GetComponent<TargetingController>();
    }

    private void ShowNoTarget()
    {
        targetNameText.text = "No Target";
        targetHPSlider.minValue = 0f;
        targetHPSlider.maxValue = 1f;
        targetHPSlider.value = 0f;
    }
}