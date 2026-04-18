using UnityEngine;

[RequireComponent(typeof(UnitStats))]
public class AutoAttackController : MonoBehaviour
{
    [SerializeField] private Transform currentTarget;
    [SerializeField] private bool useTargetingController = true;

    private UnitStats stats;
    private float attackTimer = 0f;
    private TargetingController targetingController;
    private HealthController myHealth;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
        targetingController = GetComponent<TargetingController>();
        myHealth = GetComponent<HealthController>();
    }

    private void Update()
    {
        if (myHealth != null && myHealth.IsDead())
            return;

        if (useTargetingController && targetingController != null)
        {
            currentTarget = targetingController.GetCurrentTarget();
        }

        if (currentTarget == null)
            return;

        if (!currentTarget.gameObject.activeInHierarchy)
            return;

        HealthController targetHealth = currentTarget.GetComponent<HealthController>();
        if (targetHealth != null && targetHealth.IsDead())
            return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance > stats.attackRange)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= stats.attackInterval)
        {
            attackTimer = 0f;
            PerformAttack();
        }
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    public Transform GetTarget()
    {
        return currentTarget;
    }

    private void PerformAttack()
    {
        if (currentTarget == null) return;

        IDamageable damageable = currentTarget.GetComponent<IDamageable>();
        if (damageable == null) return;

        damageable.TakeDamage(stats.attack, stats);

        ThreatTable threatTable = currentTarget.GetComponent<ThreatTable>();
        if (threatTable != null)
        {
            float generatedThreat = stats.attack * stats.threatMultiplier;
            threatTable.AddThreat(gameObject, generatedThreat);
        }

        Debug.Log($"{gameObject.name} auto-attacked {currentTarget.name} for base {stats.attack}");
    }
}