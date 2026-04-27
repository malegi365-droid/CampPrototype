using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(TargetingController))]
public class PlayerBasicAttackInputController : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.35f;

    private UnitStats stats;
    private TargetingController targetingController;
    private float attackTimer;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
        targetingController = GetComponent<TargetingController>();
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.leftButton.isPressed)
        {
            TryBasicAttack();
        }
    }

    private void TryBasicAttack()
    {
        if (attackTimer > 0f)
            return;

        Transform target = targetingController.GetCurrentTarget();

        if (target == null)
            return;

        HealthController targetHealth = target.GetComponent<HealthController>();
        if (targetHealth != null && targetHealth.IsDead())
            return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > stats.attackRange)
            return;

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
            return;

        damageable.TakeDamage(stats.attack, stats);

        ThreatTable threatTable = target.GetComponent<ThreatTable>();
        if (threatTable != null)
        {
            float generatedThreat = stats.attack * stats.threatMultiplier;
            threatTable.AddThreat(gameObject, generatedThreat);
        }

        attackTimer = attackCooldown;

        Debug.Log($"{gameObject.name} basic attacked {target.name} for {stats.attack}");
    }
}