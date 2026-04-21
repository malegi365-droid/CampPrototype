using UnityEngine;

[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(AutoAttackController))]
[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(ThreatTable))]
public class EnemyAIController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Combat,
        Returning,
        Dead
    }

    [Header("Combat")]
    [SerializeField] private float retargetInterval = 0.5f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Leash")]
    [SerializeField] private float leashDistance = 12f;
    [SerializeField] private float returnStopDistance = 0.2f;

    private EnemyState currentState = EnemyState.Idle;

    private ThreatTable threatTable;
    private AutoAttackController autoAttackController;
    private HealthController healthController;
    private UnitStats stats;

    private float retargetTimer = 0f;

    private Vector3 homePosition;
    private Quaternion homeRotation;

    private void Awake()
    {
        threatTable = GetComponent<ThreatTable>();
        autoAttackController = GetComponent<AutoAttackController>();
        healthController = GetComponent<HealthController>();
        stats = GetComponent<UnitStats>();

        homePosition = transform.position;
        homeRotation = transform.rotation;
    }

    private void Update()
    {
        if (healthController.IsDead())
        {
            currentState = EnemyState.Dead;
            autoAttackController.SetTarget(null);
            return;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;

            case EnemyState.Combat:
                HandleCombat();
                break;

            case EnemyState.Returning:
                HandleReturnHome();
                break;
        }
    }

    private void HandleIdle()
    {
        GameObject highestThreatTarget = threatTable.GetHighestThreatTarget();
        if (highestThreatTarget != null)
        {
            currentState = EnemyState.Combat;
        }
    }

    private void HandleCombat()
    {
        retargetTimer += Time.deltaTime;

        GameObject highestThreatTarget = threatTable.GetHighestThreatTarget();

        if (highestThreatTarget == null)
        {
            BeginReturnHome();
            return;
        }

        float targetDistanceFromHome = Vector3.Distance(homePosition, highestThreatTarget.transform.position);
        if (targetDistanceFromHome > leashDistance)
        {
            Debug.Log($"{gameObject.name} is leashing back home.");
            BeginReturnHome();
            return;
        }

        if (retargetTimer >= retargetInterval)
        {
            retargetTimer = 0f;
            autoAttackController.SetTarget(highestThreatTarget.transform);
        }

        float distanceToTarget = Vector3.Distance(transform.position, highestThreatTarget.transform.position);
        if (distanceToTarget > stats.attackRange)
        {
            MoveToward(highestThreatTarget.transform.position);
        }
    }

    private void HandleReturnHome()
    {
        autoAttackController.SetTarget(null);

        float distanceToHome = Vector3.Distance(transform.position, homePosition);
        if (distanceToHome > returnStopDistance)
        {
            MoveToward(homePosition);
            return;
        }

        transform.position = homePosition;
        transform.rotation = homeRotation;

        threatTable.ClearAllThreat();
        currentState = EnemyState.Idle;

        Debug.Log($"{gameObject.name} returned home.");
    }

    private void BeginReturnHome()
    {
        autoAttackController.SetTarget(null);
        threatTable.ClearAllThreat();
        currentState = EnemyState.Returning;
    }

    private void MoveToward(Vector3 destination)
    {
        Vector3 dir = destination - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            return;

        dir.Normalize();

        transform.position += dir * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    public void ForceEngage(GameObject target, float startingThreat = 5f)
    {
        if (target == null || healthController.IsDead())
            return;

        threatTable.AddThreat(target, startingThreat);
        autoAttackController.SetTarget(target.transform);
        currentState = EnemyState.Combat;
    }

    public bool IsDead()
    {
        return healthController != null && healthController.IsDead();
    }

    public bool IsBusy()
    {
        return currentState == EnemyState.Combat || currentState == EnemyState.Returning || IsDead();
    }

    public Vector3 GetHomePosition()
    {
        return homePosition;
    }

    public EnemyState GetCurrentState()
    {
        return currentState;
    }
}