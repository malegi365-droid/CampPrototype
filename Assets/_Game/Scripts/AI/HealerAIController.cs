using UnityEngine;

[RequireComponent(typeof(AbilityController))]
[RequireComponent(typeof(CharacterController))]
public class HealerAIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyControlManager partyControlManager;
    [SerializeField] private GameObject tank;
    [SerializeField] private GameObject dps;

    [Header("Healing")]
    [SerializeField] private float tankHealThreshold = 0.50f;
    [SerializeField] private float leaderHealThreshold = 0.40f;
    [SerializeField] private float selfHealThreshold = 0.50f;

    [Header("Threat Tuning")]
    [SerializeField] private float healThreatMultiplier = 0.5f;

    private AbilityController abilityController;
    private HealthController selfHealth;

    private void Awake()
    {
        abilityController = GetComponent<AbilityController>();
        selfHealth = GetComponent<HealthController>();

        if (partyControlManager == null)
            partyControlManager = FindAnyObjectByType<PartyControlManager>();
    }

    private void Update()
    {
        if (selfHealth != null && selfHealth.IsDead())
            return;

        TryHealPriorityTarget();
    }

    private void TryHealPriorityTarget()
    {
        if (tank != null)
        {
            HealthController tankHealth = tank.GetComponent<HealthController>();

            if (tankHealth != null && !tankHealth.IsDead() && tankHealth.GetHealthPercent() < tankHealThreshold)
            {
                if (abilityController.UseRestore(tank))
                    GenerateHealThreat(tank);

                return;
            }
        }

        PartyMemberControlBridge leader = partyControlManager != null ? partyControlManager.CurrentMember : null;

        if (leader != null)
        {
            HealthController leaderHealth = leader.GetComponent<HealthController>();

            if (leaderHealth != null && !leaderHealth.IsDead() && leaderHealth.GetHealthPercent() < leaderHealThreshold)
            {
                if (abilityController.UseRestore(leader.gameObject))
                    GenerateHealThreat(leader.gameObject);

                return;
            }
        }

        if (dps != null)
        {
            HealthController dpsHealth = dps.GetComponent<HealthController>();

            if (dpsHealth != null && !dpsHealth.IsDead() && dpsHealth.GetHealthPercent() < leaderHealThreshold)
            {
                if (abilityController.UseRestore(dps))
                    GenerateHealThreat(dps);

                return;
            }
        }

        if (selfHealth != null && selfHealth.GetHealthPercent() < selfHealThreshold)
        {
            if (abilityController.UseRestore(gameObject))
                GenerateHealThreat(gameObject);
        }
    }

    private void GenerateHealThreat(GameObject healedTarget)
    {
        Transform currentEnemyTarget = GetCurrentLeaderTarget();

        if (currentEnemyTarget == null)
            return;

        ThreatTable enemyThreatTable = currentEnemyTarget.GetComponent<ThreatTable>();

        if (enemyThreatTable == null)
            return;

        float healAmount = abilityController.GetLastRestoreHealAmount();
        float healThreat = healAmount * healThreatMultiplier;

        enemyThreatTable.AddThreat(gameObject, healThreat);

        Debug.Log($"{gameObject.name} generated {healThreat} heal threat on {currentEnemyTarget.name} by healing {healedTarget.name}");
    }

    private Transform GetCurrentLeaderTarget()
    {
        PartyMemberControlBridge leader = partyControlManager != null ? partyControlManager.CurrentMember : null;

        if (leader == null)
            return null;

        TargetingController leaderTargeting = leader.GetComponent<TargetingController>();

        if (leaderTargeting == null)
            return null;

        Transform target = leaderTargeting.GetCurrentTarget();

        if (target == null)
            return null;

        HealthController targetHealth = target.GetComponent<HealthController>();

        if (targetHealth != null && targetHealth.IsDead())
            return null;

        return target.gameObject.activeInHierarchy ? target : null;
    }

    public void SetReferences(GameObject tankObject, GameObject dpsObject)
    {
        tank = tankObject;
        dps = dpsObject;
    }
}