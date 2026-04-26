using TMPro;
using UnityEngine;

public class ThreatDebugUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyControlManager partyControlManager;
    [SerializeField] private GameObject dps;
    [SerializeField] private GameObject tank;
    [SerializeField] private GameObject healer;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI threatTitleText;
    [SerializeField] private TextMeshProUGUI threatEnemyTargetText;
    [SerializeField] private TextMeshProUGUI threatDpsText;
    [SerializeField] private TextMeshProUGUI threatTankText;
    [SerializeField] private TextMeshProUGUI threatHealerText;

    private void Awake()
    {
        if (partyControlManager == null)
            partyControlManager = FindAnyObjectByType<PartyControlManager>();
    }

    private void Update()
    {
        TargetingController activeTargeting = GetActiveTargeting();

        if (activeTargeting == null)
        {
            ShowNoTarget();
            return;
        }

        Transform currentTarget = activeTargeting.GetCurrentTarget();

        if (currentTarget == null)
        {
            ShowNoTarget();
            return;
        }

        UnitStats enemyStats = currentTarget.GetComponent<UnitStats>();
        ThreatTable threatTable = currentTarget.GetComponent<ThreatTable>();
        AutoAttackController enemyAttack = currentTarget.GetComponent<AutoAttackController>();
        HealthController enemyHealth = currentTarget.GetComponent<HealthController>();

        if (enemyStats == null || threatTable == null || enemyHealth == null || enemyHealth.IsDead())
        {
            ShowNoTarget();
            return;
        }

        if (threatTitleText != null)
            threatTitleText.text = $"Threat: {enemyStats.unitName}";

        string enemyCurrentTargetName = "None";
        if (enemyAttack != null && enemyAttack.GetTarget() != null)
            enemyCurrentTargetName = enemyAttack.GetTarget().name;

        if (threatEnemyTargetText != null)
            threatEnemyTargetText.text = $"Enemy Target: {enemyCurrentTargetName}";

        float dpsThreat = threatTable.GetThreatFor(dps);
        float tankThreat = threatTable.GetThreatFor(tank);
        float healerThreat = threatTable.GetThreatFor(healer);

        if (threatDpsText != null)
            threatDpsText.text = $"DPS: {dpsThreat:F1}";

        if (threatTankText != null)
            threatTankText.text = $"Tank: {tankThreat:F1}";

        if (threatHealerText != null)
            threatHealerText.text = $"Healer: {healerThreat:F1}";
    }

    private TargetingController GetActiveTargeting()
    {
        if (partyControlManager == null || partyControlManager.CurrentMember == null)
            return null;

        return partyControlManager.CurrentMember.GetComponent<TargetingController>();
    }

    private void ShowNoTarget()
    {
        if (threatTitleText != null)
            threatTitleText.text = "Threat: No Target";

        if (threatEnemyTargetText != null)
            threatEnemyTargetText.text = "Enemy Target: None";

        if (threatDpsText != null)
            threatDpsText.text = "DPS: 0";

        if (threatTankText != null)
            threatTankText.text = "Tank: 0";

        if (threatHealerText != null)
            threatHealerText.text = "Healer: 0";
    }
}