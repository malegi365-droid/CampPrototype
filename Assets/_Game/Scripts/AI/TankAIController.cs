using UnityEngine;

[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(AutoAttackController))]
[RequireComponent(typeof(AbilityController))]
[RequireComponent(typeof(CharacterController))]
public class TankAIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyControlManager partyControlManager;
    [SerializeField] private GameObject healer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Combat")]
    [SerializeField] private float awarenessRadius = 25f;
    [SerializeField] private float maxDistanceFromLeader = 10f;
    [SerializeField] private float assistLeaderTargetMaxDistance = 8f;

    private AbilityController abilityController;
    private AutoAttackController autoAttackController;
    private HealthController healthController;
    private UnitStats myStats;
    private CharacterController characterController;

    private void Awake()
    {
        abilityController = GetComponent<AbilityController>();
        autoAttackController = GetComponent<AutoAttackController>();
        healthController = GetComponent<HealthController>();
        myStats = GetComponent<UnitStats>();
        characterController = GetComponent<CharacterController>();

        if (partyControlManager == null)
            partyControlManager = FindAnyObjectByType<PartyControlManager>();
    }

    private void Update()
    {
        if (healthController != null && healthController.IsDead())
            return;

        PartyMemberControlBridge leader = partyControlManager != null ? partyControlManager.CurrentMember : null;

        if (leader == null)
            return;

        float distanceFromLeader = FlatDistance(transform.position, leader.transform.position);

        if (distanceFromLeader > maxDistanceFromLeader)
        {
            autoAttackController.SetTarget(null);
            return;
        }

        Transform priorityTarget = GetPriorityTarget(leader);

        if (priorityTarget == null)
        {
            autoAttackController.SetTarget(null);
            return;
        }

        autoAttackController.SetTarget(priorityTarget);

        float distanceToTarget = FlatDistance(transform.position, priorityTarget.position);
        if (distanceToTarget > myStats.attackRange)
        {
            MoveToward(priorityTarget.position);
        }

        abilityController.UseGuardingStrike(priorityTarget);
    }

    private Transform GetPriorityTarget(PartyMemberControlBridge leader)
    {
        Transform enemyAttackingLeader = FindEnemyTargeting(leader.transform);
        if (enemyAttackingLeader != null)
            return enemyAttackingLeader;

        if (healer != null)
        {
            Transform enemyAttackingHealer = FindEnemyTargeting(healer.transform);
            if (enemyAttackingHealer != null)
                return enemyAttackingHealer;
        }

        Transform enemyAttackingTank = FindEnemyTargeting(transform);
        if (enemyAttackingTank != null)
            return enemyAttackingTank;

        TargetingController leaderTargeting = leader.GetComponent<TargetingController>();
        Transform leaderTarget = leaderTargeting != null ? leaderTargeting.GetCurrentTarget() : null;

        if (leaderTarget != null && leaderTarget.gameObject.activeInHierarchy)
        {
            HealthController targetHealth = leaderTarget.GetComponent<HealthController>();

            if (targetHealth == null || !targetHealth.IsDead())
            {
                float distanceToLeaderTarget = FlatDistance(transform.position, leaderTarget.position);

                if (distanceToLeaderTarget <= assistLeaderTargetMaxDistance)
                    return leaderTarget;
            }
        }

        return null;
    }

    private Transform FindEnemyTargeting(Transform partyMember)
    {
        if (partyMember == null)
            return null;

        Collider[] hits = Physics.OverlapSphere(transform.position, awarenessRadius);

        Transform bestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            UnitStats stats = hit.GetComponent<UnitStats>();
            AutoAttackController enemyAttack = hit.GetComponent<AutoAttackController>();
            HealthController enemyHealth = hit.GetComponent<HealthController>();

            if (stats == null || enemyAttack == null || enemyHealth == null)
                continue;

            if (stats.role != UnitRole.Enemy)
                continue;

            if (enemyHealth.IsDead())
                continue;

            Transform enemyTarget = enemyAttack.GetTarget();
            if (enemyTarget != partyMember)
                continue;

            float dist = FlatDistance(transform.position, hit.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestEnemy = hit.transform;
            }
        }

        return bestEnemy;
    }

    private void MoveToward(Vector3 destination)
    {
        Vector3 dir = destination - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            return;

        dir.Normalize();

        characterController.Move(dir * moveSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    public void SetReferences(GameObject healerObject)
    {
        healer = healerObject;
    }
}