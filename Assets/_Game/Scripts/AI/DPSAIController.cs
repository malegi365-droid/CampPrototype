using UnityEngine;

[RequireComponent(typeof(AutoAttackController))]
[RequireComponent(typeof(AbilityController))]
[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(CharacterController))]
public class DPSAIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyControlManager partyControlManager;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Combat")]
    [SerializeField] private float maxAssistDistanceFromTarget = 25f;

    private AutoAttackController autoAttackController;
    private AbilityController abilityController;
    private HealthController healthController;
    private UnitStats stats;
    private CharacterController characterController;

    private void Awake()
    {
        autoAttackController = GetComponent<AutoAttackController>();
        abilityController = GetComponent<AbilityController>();
        healthController = GetComponent<HealthController>();
        stats = GetComponent<UnitStats>();
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

        Transform target = GetLeaderTarget(leader);

        if (target == null)
        {
            autoAttackController.SetTarget(null);
            return;
        }

        float distanceToTarget = FlatDistance(transform.position, target.position);

        if (distanceToTarget > maxAssistDistanceFromTarget)
        {
            autoAttackController.SetTarget(null);
            return;
        }

        autoAttackController.SetTarget(target);

        if (distanceToTarget > stats.attackRange)
        {
            MoveToward(target.position);
            return;
        }

        abilityController.UseHeavyShot();

        if (abilityController.HasFocusBreak())
            abilityController.UseFocusBreak();
    }

    private Transform GetLeaderTarget(PartyMemberControlBridge leader)
    {
        TargetingController leaderTargeting = leader.GetComponent<TargetingController>();

        if (leaderTargeting == null)
            return null;

        Transform target = leaderTargeting.GetCurrentTarget();

        if (target == null || !target.gameObject.activeInHierarchy)
            return null;

        HealthController targetHealth = target.GetComponent<HealthController>();
        if (targetHealth != null && targetHealth.IsDead())
            return null;

        UnitStats targetStats = target.GetComponent<UnitStats>();
        if (targetStats == null || targetStats.role != UnitRole.Enemy)
            return null;

        return target;
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
}