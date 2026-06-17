using System.Collections.Generic;
using UnityEngine;

public class ToxicProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;

    [Header("Hit Detection")]
    [SerializeField] private float hitRadius = 0.2f;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private float armTime = 0.1f;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 0.6f;

    [Header("Poison")]
    [SerializeField] private bool appliesPoison = true;
    [SerializeField] private float poisonDuration = 5f;
    [SerializeField] private float poisonDamagePerTick = 5f;

    [Header("Impact")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private GameObject hitSparkEffectPrefab;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float impactVolume = 1f;

    [Header("Hit Reaction")]
    [SerializeField] private float staggerDuration = 0.15f;
    [SerializeField] private float knockbackStrength = 0.4f;

    private Vector3 travelDirection;
    private float spawnTime;
    private UnitStats shooterStats;
    private TargetingController shooterTargeting;

    private readonly HashSet<Transform> alreadyHitRoots = new();

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
                    if (HandleHit(hit))
                        return;
                }
            }
        }

        transform.position += travelDirection * moveDistance;
    }

    private bool HandleHit(RaycastHit hit)
    {
        Transform enemyRoot = GetEnemyRoot(hit.collider.transform);

        if (enemyRoot != null && alreadyHitRoots.Contains(enemyRoot))
            return false;

        if (enemyRoot != null)
            alreadyHitRoots.Add(enemyRoot);

        SpawnImpactEffects(hit);

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
            damageable.TakeDamage(damageAmount, shooterStats);

            ApplyPoison(hit.collider.transform);
            ApplyCombatPolish(hit.collider.transform, travelDirection);
        }

        Destroy(gameObject);
        return true;
    }

    private void ApplyPoison(Transform hitTransform)
    {
        if (!appliesPoison || hitTransform == null)
            return;

        PoisonStatusEffect poison =
            hitTransform.GetComponentInParent<PoisonStatusEffect>();

        if (poison == null)
            poison = hitTransform.GetComponentInChildren<PoisonStatusEffect>();

        if (poison != null)
            poison.ApplyPoison(poisonDuration, poisonDamagePerTick);
    }

    private void ApplyCombatPolish(Transform hitTransform, Vector3 direction)
    {
        EnemyHitFlash hitFlash = hitTransform.GetComponentInParent<EnemyHitFlash>();

        if (hitFlash == null)
            hitFlash = hitTransform.GetComponentInChildren<EnemyHitFlash>();

        if (hitFlash != null)
            hitFlash.TriggerFlash();

        EnemyHitReactionController reaction =
            hitTransform.GetComponentInParent<EnemyHitReactionController>();

        if (reaction == null)
            reaction = hitTransform.GetComponentInChildren<EnemyHitReactionController>();

        if (reaction != null)
        {
            reaction.ApplyHitReaction(
                direction,
                knockbackStrength,
                staggerDuration,
                false
            );
        }

        if (HitStopManager.Instance != null)
            HitStopManager.Instance.DoHitStop(0.02f);
    }

    private void SpawnImpactEffects(RaycastHit hit)
    {
        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, hit.point, Quaternion.identity);

        if (hitSparkEffectPrefab != null)
            Instantiate(hitSparkEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
    }

    private Transform GetEnemyRoot(Transform candidate)
    {
        if (candidate == null)
            return null;

        UnitStats stats = candidate.GetComponent<UnitStats>();

        if (stats == null)
            stats = candidate.GetComponentInParent<UnitStats>();

        if (stats == null || stats.role != UnitRole.Enemy)
            return null;

        return stats.transform;
    }
}