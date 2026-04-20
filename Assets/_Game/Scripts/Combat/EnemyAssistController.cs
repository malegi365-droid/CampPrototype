using UnityEngine;

[RequireComponent(typeof(ThreatTable))]
[RequireComponent(typeof(EnemyAIController))]
public class EnemyAssistController : MonoBehaviour
{
    [SerializeField] private float assistRadius = 6f;
    [SerializeField] private float assistThreat = 8f;
    [SerializeField] private bool assistOncePerCombat = true;
    [SerializeField] private bool debugAssistLogs = true;

    private ThreatTable threatTable;
    private EnemyAIController enemyAI;
    private bool hasCalledForAssist = false;

    private void Awake()
    {
        threatTable = GetComponent<ThreatTable>();
        enemyAI = GetComponent<EnemyAIController>();
    }

    private void Update()
    {
        if (enemyAI == null || enemyAI.IsDead())
            return;

        GameObject currentTarget = threatTable.GetHighestThreatTarget();
        if (currentTarget == null)
        {
            hasCalledForAssist = false;
            return;
        }

        if (assistOncePerCombat && hasCalledForAssist)
            return;

        CallNearbyAllies(currentTarget);
        hasCalledForAssist = true;
    }

    private void CallNearbyAllies(GameObject target)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, assistRadius);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            UnitStats otherStats = hit.GetComponent<UnitStats>();
            EnemyAIController otherEnemyAI = hit.GetComponent<EnemyAIController>();
            HealthController otherHealth = hit.GetComponent<HealthController>();

            if (otherStats == null || otherEnemyAI == null || otherHealth == null)
                continue;

            if (otherStats.role != UnitRole.Enemy)
                continue;

            if (otherHealth.IsDead())
                continue;

            otherEnemyAI.ForceEngage(target, assistThreat);

            if (debugAssistLogs)
            {
                Debug.Log($"{gameObject.name} called {hit.gameObject.name} to assist against {target.name}");
            }
        }
    }
}