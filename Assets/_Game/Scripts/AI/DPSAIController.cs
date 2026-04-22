using UnityEngine;

[RequireComponent(typeof(AutoAttackController))]
[RequireComponent(typeof(AbilityController))]
[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(UnitStats))]
public class DPSAIController : MonoBehaviour
{
    [SerializeField] private Transform playerAnchor;
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float moveSpeed = 4f;

    private AutoAttackController autoAttackController;
    private AbilityController abilityController;
    private HealthController healthController;
    private TargetingController playerTargeting;
    private UnitStats stats;

    [SerializeField] private bool aiEnabled = false;

    private void Awake()
    {
        autoAttackController = GetComponent<AutoAttackController>();
        abilityController = GetComponent<AbilityController>();
        healthController = GetComponent<HealthController>();
        stats = GetComponent<UnitStats>();

        if (playerAnchor != null)
        {
            playerTargeting = playerAnchor.GetComponent<TargetingController>();
        }
    }

    private void Update()
    {
        if (!aiEnabled) return;
        if (healthController != null && healthController.IsDead()) return;

        Transform target = playerTargeting != null ? playerTargeting.GetCurrentTarget() : null;

        if (target != null)
        {
            autoAttackController.SetTarget(target);

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > stats.attackRange)
            {
                MoveToward(target.position);
            }

            abilityController.UseHeavyShot();

            if (abilityController.HasFocusBreak())
            {
                abilityController.UseFocusBreak();
            }
        }
        else
        {
            autoAttackController.SetTarget(null);

            if (playerAnchor != null)
            {
                float distanceToAnchor = Vector3.Distance(transform.position, playerAnchor.position);
                if (distanceToAnchor > followDistance)
                {
                    MoveToward(playerAnchor.position);
                }
            }
        }
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
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
    }

    public void SetAIEnabled(bool enabled)
    {
        aiEnabled = enabled;
    }

    public bool IsAIEnabled()
    {
        return aiEnabled;
    }

    public void SetPlayerAnchor(Transform anchor)
    {
        playerAnchor = anchor;
        playerTargeting = playerAnchor != null ? playerAnchor.GetComponent<TargetingController>() : null;
    }
}