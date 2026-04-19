using UnityEngine;

[RequireComponent(typeof(AbilityController))]
public class HealerAIController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject tank;
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Threat Tuning")]
    [SerializeField] private float healThreatMultiplier = 0.5f;

    private AbilityController abilityController;
    private HealthController selfHealth;
    private TargetingController playerTargeting;

    private void Awake()
    {
        abilityController = GetComponent<AbilityController>();
        selfHealth = GetComponent<HealthController>();

        if (player != null)
        {
            playerTargeting = player.GetComponent<TargetingController>();
        }
    }

    private void Update()
    {
        if (selfHealth != null && selfHealth.IsDead())
            return;

        TryHealPriorityTarget();
        FollowPartyAnchor();
    }

    private void TryHealPriorityTarget()
    {
        if (tank != null)
        {
            HealthController tankHealth = tank.GetComponent<HealthController>();
            if (tankHealth != null && !tankHealth.IsDead() && tankHealth.GetHealthPercent() < 0.50f)
            {
                if (abilityController.UseRestore(tank))
                {
                    GenerateHealThreat(tank);
                }
                return;
            }
        }

        if (player != null)
        {
            HealthController playerHealth = player.GetComponent<HealthController>();
            if (playerHealth != null && !playerHealth.IsDead() && playerHealth.GetHealthPercent() < 0.40f)
            {
                if (abilityController.UseRestore(player.gameObject))
                {
                    GenerateHealThreat(player.gameObject);
                }
                return;
            }
        }

        if (selfHealth != null && selfHealth.GetHealthPercent() < 0.50f)
        {
            if (abilityController.UseRestore(gameObject))
            {
                GenerateHealThreat(gameObject);
            }
        }
    }

    private void GenerateHealThreat(GameObject healedTarget)
    {
        if (playerTargeting == null) return;

        Transform currentEnemyTarget = playerTargeting.GetCurrentTarget();
        if (currentEnemyTarget == null) return;

        ThreatTable enemyThreatTable = currentEnemyTarget.GetComponent<ThreatTable>();
        if (enemyThreatTable == null) return;

        float healAmount = abilityController.GetLastRestoreHealAmount();
        float healThreat = healAmount * healThreatMultiplier;

        enemyThreatTable.AddThreat(gameObject, healThreat);

        Debug.Log($"{gameObject.name} generated {healThreat} heal threat on {currentEnemyTarget.name} by healing {healedTarget.name}");
    }

    private void FollowPartyAnchor()
    {
        Vector3 anchorPosition = transform.position;

        if (tank != null)
        {
            anchorPosition = tank.transform.position;
        }
        else if (player != null)
        {
            anchorPosition = player.position;
        }

        float distance = Vector3.Distance(transform.position, anchorPosition);
        if (distance > followDistance)
        {
            Vector3 dir = (anchorPosition - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
    }

    public void SetReferences(Transform playerTransform, GameObject tankObject)
    {
        player = playerTransform;
        tank = tankObject;
        playerTargeting = player != null ? player.GetComponent<TargetingController>() : null;
    }
}