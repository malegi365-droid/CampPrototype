using UnityEngine;

[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(AutoAttackController))]
[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(ThreatTable))]
public class EnemyAIController : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Combat,
        Dead
    }

    [SerializeField] private EnemyState currentState = EnemyState.Idle;
    [SerializeField] private float retargetInterval = 0.5f;

    private ThreatTable threatTable;
    private AutoAttackController autoAttackController;
    private HealthController healthController;

    private float retargetTimer = 0f;

    private void Awake()
    {
        threatTable = GetComponent<ThreatTable>();
        autoAttackController = GetComponent<AutoAttackController>();
        healthController = GetComponent<HealthController>();
    }

    private void Update()
    {
        if (healthController.IsDead())
        {
            currentState = EnemyState.Dead;
            autoAttackController.SetTarget(null);
            return;
        }

        retargetTimer += Time.deltaTime;

        GameObject highestThreatTarget = threatTable.GetHighestThreatTarget();

        if (highestThreatTarget == null)
        {
            currentState = EnemyState.Idle;
            autoAttackController.SetTarget(null);
            return;
        }

        currentState = EnemyState.Combat;

        if (retargetTimer >= retargetInterval)
        {
            retargetTimer = 0f;
            autoAttackController.SetTarget(highestThreatTarget.transform);
        }
    }
}