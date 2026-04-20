using UnityEngine;

[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(AutoAttackController))]
[RequireComponent(typeof(AbilityController))]
public class TankAIController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject healer;
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float awarenessRadius = 25f;

    [Header("Disengage")]
    [SerializeField] private float maxDistanceFromPlayer = 6f;

    private AbilityController abilityController;
    private AutoAttackController autoAttackController;
    private TargetingController playerTargeting;
    private HealthController healthController;
    private UnitStats myStats;

    private void Awake()
    {
        abilityController = GetComponent<AbilityController>();
        autoAttackController = GetComponent<AutoAttackController>();
        healthController = GetComponent<HealthController>();
        myStats = GetComponent<UnitStats>();

        if (player != null)
        {
            playerTargeting = player.GetComponent<TargetingController>();
        }
    }

    private void Update()
    {
        if (healthController != null && healthController.IsDead())
            return;

        if (player == null)
            return;

        float distanceFromPlayer = Vector3.Distance(transform.position, player.position);

        // If the tank gets too far from the player, break off and return.
        if (distanceFromPlayer > maxDistanceFromPlayer)
        {
            autoAttackController.SetTarget(null);
            FollowPlayer(forceFollow: true);
            return;
        }

        Transform priorityTarget = GetPriorityTarget();

        if (priorityTarget == null)
        {
            FollowPlayer(forceFollow: false);
            autoAttackController.SetTarget(null);
            return;
        }

        autoAttackController.SetTarget(priorityTarget);

        float distanceToTarget = Vector3.Distance(transform.position, priorityTarget.position);
        if (distanceToTarget > myStats.attackRange)
        {
            MoveToward(priorityTarget.position);
        }

        abilityController.UseGuardingStrike(priorityTarget);
    }

    private Transform GetPriorityTarget()
    {
        Transform targetAttackingPlayer = FindEnemyTargeting(player);
        if (targetAttackingPlayer != null)
            return targetAttackingPlayer;

        if (healer != null)
        {
            Transform targetAttackingHealer = FindEnemyTargeting(healer.transform);
            if (targetAttackingHealer != null)
                return targetAttackingHealer;
        }

        Transform targetAttackingTank = FindEnemyTargeting(transform);
        if (targetAttackingTank != null)
            return targetAttackingTank;

        Transform playerSelectedTarget = playerTargeting != null ? playerTargeting.GetCurrentTarget() : null;
        if (playerSelectedTarget != null && playerSelectedTarget.gameObject.activeInHierarchy)
        {
            HealthController targetHealth = playerSelectedTarget.GetComponent<HealthController>();
            if (targetHealth == null || !targetHealth.IsDead())
            {
                return playerSelectedTarget;
            }
        }

        return null;
    }

    private Transform FindEnemyTargeting(Transform partyMember)
    {
        if (partyMember == null) return null;

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

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestEnemy = hit.transform;
            }
        }

        return bestEnemy;
    }

    private void FollowPlayer(bool forceFollow)
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (forceFollow || distance > followDistance)
        {
            MoveToward(player.position);
        }
    }

    private void MoveToward(Vector3 destination)
    {
        Vector3 dir = (destination - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void SetReferences(Transform playerTransform, GameObject healerObject)
    {
        player = playerTransform;
        healer = healerObject;
        playerTargeting = player != null ? player.GetComponent<TargetingController>() : null;
    }
}