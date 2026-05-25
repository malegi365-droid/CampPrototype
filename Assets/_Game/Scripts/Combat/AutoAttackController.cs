using UnityEngine;

[RequireComponent(typeof(UnitStats))]
public class AutoAttackController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform currentTarget;

    [Header("Attack Feel")]
    [SerializeField] private float initialAttackDelay = 0.35f;
    [SerializeField] private bool faceTargetBeforeAttack = true;

    private UnitStats stats;
    private float attackTimer = 0f;
    private HealthController myHealth;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
        myHealth = GetComponent<HealthController>();

        attackTimer = initialAttackDelay;
    }

    private void Update()
    {
        if (myHealth != null && myHealth.IsDead())
            return;

        if (currentTarget == null)
            return;

        if (!currentTarget.gameObject.activeInHierarchy)
            return;

        HealthController targetHealth = currentTarget.GetComponent<HealthController>();
        if (targetHealth != null && targetHealth.IsDead())
            return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance > stats.attackRange)
        {
            attackTimer = Mathf.Min(attackTimer, initialAttackDelay);
            return;
        }

        if (faceTargetBeforeAttack)
            FaceTarget();

        attackTimer += Time.deltaTime;

        if (attackTimer >= stats.attackInterval)
        {
            attackTimer = 0f;
            PerformAttack();
        }
    }

    public void SetTarget(Transform target)
    {
        // Prevent constantly resetting attack timing
        // when EnemyAIController refreshes the same target.
        if (currentTarget == target)
            return;

        currentTarget = target;

        if (currentTarget != null)
            attackTimer = initialAttackDelay;
    }

    public Transform GetTarget()
    {
        return currentTarget;
    }

    private void FaceTarget()
    {
        if (currentTarget == null)
            return;

        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized, Vector3.up);

        transform.rotation =
            Quaternion.Slerp(transform.rotation, targetRotation, 12f * Time.deltaTime);
    }

    private void PerformAttack()
    {
        if (currentTarget == null)
            return;

        IDamageable damageable =
            currentTarget.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = currentTarget.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        damageable.TakeDamage(stats.attack, stats);

        ThreatTable threatTable =
            currentTarget.GetComponent<ThreatTable>();

        if (threatTable == null)
            threatTable = currentTarget.GetComponentInParent<ThreatTable>();

        if (threatTable != null)
        {
            float generatedThreat =
                stats.attack * stats.threatMultiplier;

            threatTable.AddThreat(gameObject, generatedThreat);
        }

        Debug.Log($"{gameObject.name} attacked {currentTarget.name} for base {stats.attack}");
    }
}