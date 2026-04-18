using UnityEngine;

[RequireComponent(typeof(AbilityController))]
public class HealerAIController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject tank;
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float moveSpeed = 3.5f;

    private AbilityController abilityController;
    private HealthController selfHealth;

    private void Awake()
    {
        abilityController = GetComponent<AbilityController>();
        selfHealth = GetComponent<HealthController>();
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
                abilityController.UseRestore(tank);
                return;
            }
        }

        if (player != null)
        {
            HealthController playerHealth = player.GetComponent<HealthController>();
            if (playerHealth != null && !playerHealth.IsDead() && playerHealth.GetHealthPercent() < 0.40f)
            {
                abilityController.UseRestore(player.gameObject);
                return;
            }
        }

        if (selfHealth != null && selfHealth.GetHealthPercent() < 0.50f)
        {
            abilityController.UseRestore(gameObject);
        }
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
    }
}