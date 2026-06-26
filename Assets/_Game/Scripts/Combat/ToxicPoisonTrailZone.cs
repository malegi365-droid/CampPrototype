using UnityEngine;

public class ToxicPoisonTrailZone : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifetime = 2.5f;

    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 4f;
    [SerializeField] private float tickInterval = 0.35f;

    [Header("Detection")]
    [SerializeField] private float radius = 1.4f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("VFX")]
    [SerializeField] private GameObject visualRoot;

    private float tickTimer;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            DamageEnemiesInTrail();
        }
    }

    private void DamageEnemiesInTrail()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            UnitStats stats = hit.GetComponentInParent<UnitStats>();

            if (stats == null || stats.role != UnitRole.Enemy)
                continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            float damage = damagePerSecond * tickInterval;
            damageable.TakeDamage(damage, null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}