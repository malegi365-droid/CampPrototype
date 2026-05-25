using System.Collections.Generic;
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
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float impactVolume = 1f;

    [Header("Piercing")]
    [SerializeField] private bool piercing = false;
    [SerializeField] private int maxPierceHits = 3;
    [SerializeField] private float piercingDamageFalloff = 0.85f;

    [Header("Hit Reaction")]
    [SerializeField] private float staggerDuration = 0.12f;
    [SerializeField] private float knockbackStrength = 0.35f;

    private Vector3 travelDirection;
    private float spawnTime;
    private UnitStats shooterStats;
    private TargetingController shooterTargeting;

    private readonly HashSet<Transform> alreadyHitRoots = new HashSet<Transform>();
    private int pierceHits = 0;

    public void Initialize(Vector3 direction, UnitStats ownerStats, TargetingController ownerTargeting)
    {
        travelDirection = direction.normalized;
        shooterStats = ownerStats;
        shooterTargeting = ownerTargeting;
        spawnTime = Time.time;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        float moveDistance = speed * Time.deltaTime;

        if (Time.time - spawnTime >= armTime)
        {
            RaycastHit[] hits = Physics.SphereCastAll(
                transform.position,
                hitRadius,
                travelDirection,
                moveDistance,
                hitLayers,
                QueryTriggerInteraction.Ignore
            );

            if (hits != null && hits.Length > 0)
            {
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                foreach (RaycastHit hit in hits)
                {
                    bool shouldStop = HandleHit(hit);

                    if (shouldStop)
                        return;
                }
            }
        }

        transform.position += travelDirection * moveDistance;
    }

    private bool HandleHit(RaycastHit hit)
    {
        Debug.Log($"Projectile hit: {hit.collider.name}");

        Transform enemyRoot = GetEnemyRoot(hit.collider.transform);

        if (piercing && enemyRoot != null)
        {
            if (alreadyHitRoots.Contains(enemyRoot))
                return false;

            alreadyHitRoots.Add(enemyRoot);
        }

        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, hit.point, Quaternion.identity);

        if (impactSound != null)
            AudioSource.PlayClipAtPoint(impactSound, hit.point, impactVolume);

        if (enemyRoot != null && shooterTargeting != null)
            shooterTargeting.SetTarget(enemyRoot);

        IDamageable damageable = hit.collider.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null && shooterStats != null)
        {
            float damageAmount = shooterStats.attack * damageMultiplier;

            if (piercing)
                damageAmount *= Mathf.Pow(piercingDamageFalloff, pierceHits);

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

        if (!piercing)
        {
            Destroy(gameObject);
            return true;
        }

        pierceHits++;

        if (pierceHits >= maxPierceHits)
        {
            Destroy(gameObject);
            return true;
        }

        return false;
    }

    private Transform GetEnemyRoot(Transform candidate)
    {
        if (candidate == null)
            return null;

        UnitStats stats = candidate.GetComponent<UnitStats>();

        if (stats == null)
            stats = candidate.GetComponentInParent<UnitStats>();

        if (stats == null)
            return null;

        if (stats.role != UnitRole.Enemy)
            return null;

        return stats.transform;
    }
}