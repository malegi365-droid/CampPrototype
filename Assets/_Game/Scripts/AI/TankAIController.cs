using UnityEngine;

[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(AutoAttackController))]
[RequireComponent(typeof(AbilityController))]
public class TankAIController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float moveSpeed = 4f;

    private AbilityController abilityController;
    private AutoAttackController autoAttackController;
    private TargetingController playerTargeting;
    private HealthController healthController;

    private void Awake()
    {
        abilityController = GetComponent<AbilityController>();
        autoAttackController = GetComponent<AutoAttackController>();
        healthController = GetComponent<HealthController>();

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

        Transform playerTarget = playerTargeting != null ? playerTargeting.GetCurrentTarget() : null;

        if (playerTarget == null)
        {
            FollowPlayer();
            autoAttackController.SetTarget(null);
            return;
        }

        autoAttackController.SetTarget(playerTarget);

        float distanceToTarget = Vector3.Distance(transform.position, playerTarget.position);
        float myAttackRange = GetComponent<UnitStats>().attackRange;

        if (distanceToTarget > myAttackRange)
        {
            MoveToward(playerTarget.position);
        }

        abilityController.UseGuardingStrike(playerTarget);
    }

    private void FollowPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > followDistance)
        {
            MoveToward(player.position);
        }
    }

    private void MoveToward(Vector3 destination)
    {
        Vector3 dir = (destination - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        playerTargeting = player != null ? player.GetComponent<TargetingController>() : null;
    }
}