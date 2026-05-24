using UnityEngine;

public class DPSInjectorProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;

    [Header("Hit Detection")]
    [SerializeField] private float hitRadius = 0.2f;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private float armTime = 0.1f;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 1f;

    [Header("Impact")]
    [SerializeField] private GameObject impactEffectPrefab;

    [Header("Hit Reaction")]
    [SerializeField] private float staggerDuration = 0.12f;
    [SerializeField] private float knockbackStrength = 0.35f;

    private Vector3 travelDirection;
    private float spawnTime;
    private UnitStats shooterStats;

    public void Initialize(Vector3 direction, UnitStats ownerStats)
    {
        travelDirection = direction.normalized;
        shooterStats = ownerStats;
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        float moveDistance = speed * Time.deltaTime;

        if (Time.time - spawnTime >= armTime)
        {
            if (Physics.SphereCast(
                transform.position,
                hitRadius,
                travelDirection,
                out RaycastHit hit,
                moveDistance,
                hitLayers,
                QueryTriggerInteraction.Ignore
            ))
            {
                HandleHit(hit);
                return;
            }
        }

        transform.position += travelDirection * moveDistance;
    }

    private void HandleHit(RaycastHit hit)
    {
        Debug.Log($"Projectile hit: {hit.collider.name}");

        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, hit.point, Quaternion.identity);

        IDamageable damageable = hit.collider.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null && shooterStats != null)
        {
            float damageAmount = shooterStats.attack * damageMultiplier;
            damageable.TakeDamage(damageAmount, shooterStats);
        }

        EnemyHitReactionController reaction =
            hit.collider.GetComponent<EnemyHitReactionController>();

        if (reaction == null)
            reaction = hit.collider.GetComponentInParent<EnemyHitReactionController>();

        if (reaction == null)
            reaction = hit.collider.GetComponentInChildren<EnemyHitReactionController>();

        if (reaction != null)
        {
            reaction.ApplyStagger(staggerDuration);
            reaction.ApplyKnockback(travelDirection, knockbackStrength);
        }

        Destroy(gameObject);
    }
}