using System.Collections.Generic;
using UnityEngine;

public class RangerInjectorProjectile : MonoBehaviour
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

    [Header("Projectile Type")]
    [SerializeField] private bool overchargeProjectile = false;

    [Header("Impact")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private GameObject hitSparkEffectPrefab;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float impactVolume = 1f;

    [Header("Impact Camera Shake")]
    [SerializeField] private bool enableImpactCameraShake = true;
    [SerializeField] private float impactShakeDuration = 0.05f;
    [SerializeField] private float impactShakeStrength = 0.05f;
    [SerializeField] private float explosiveImpactShakeDuration = 0.12f;
    [SerializeField] private float explosiveImpactShakeStrength = 0.12f;
    [SerializeField] private float overchargeImpactShakeDuration = 0.08f;
    [SerializeField] private float overchargeImpactShakeStrength = 0.08f;

    [Header("Hit Stop")]
    [SerializeField] private bool enableHitStop = true;
    [SerializeField] private float normalHitStopDuration = 0.025f;
    [SerializeField] private float overchargeHitStopDuration = 0.045f;
    [SerializeField] private float explosiveHitStopDuration = 0.04f;

    [Header("Piercing")]
    [SerializeField] private bool piercing = false;
    [SerializeField] private int maxPierceHits = 3;
    [SerializeField] private float piercingDamageFalloff = 0.85f;

    [Header("Explosion")]
    [SerializeField] private bool explosive = false;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float explosionDamageMultiplier = 0.75f;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Hit Reaction")]
    [SerializeField] private float staggerDuration = 0.2f;
    [SerializeField] private float knockbackStrength = 1f;
    [SerializeField] private float explosiveStaggerMultiplier = 1.5f;
    [SerializeField] private float explosiveKnockbackMultiplier = 2f;

    private Vector3 travelDirection;
    private float spawnTime;
    private UnitStats shooterStats;
    private TargetingController shooterTargeting;
    private CameraShakeController cameraShake;

    private readonly HashSet<Transform> alreadyHitRoots = new HashSet<Transform>();
    private int pierceHits = 0;

    public void Initialize(Vector3 direction, UnitStats ownerStats, TargetingController ownerTargeting)
    {
        travelDirection = direction.normalized;
        shooterStats = ownerStats;
        shooterTargeting = ownerTargeting;
        spawnTime = Time.time;

        Debug.Log($"[RangerInjectorProjectile] Initialized. Direction={travelDirection}, Speed={speed}");

        cameraShake = FindAnyObjectByType<CameraShakeController>();

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

        SpawnImpactEffects(hit);
        TriggerImpactCameraShake();

        if (impactSound != null)
            AudioSource.PlayClipAtPoint(impactSound, hit.point, impactVolume);

        if (explosive)
            Explode(hit.point, enemyRoot);

        if (enemyRoot != null && shooterTargeting != null)
            shooterTargeting.SetTarget(enemyRoot);

        IDamageable damageable = hit.collider.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null && shooterStats != null && !explosive)
        {
            float damageAmount = shooterStats.attack * damageMultiplier;

            if (piercing)
                damageAmount *= Mathf.Pow(piercingDamageFalloff, pierceHits);

            damageable.TakeDamage(damageAmount, shooterStats);

            ApplyCombatPolish(hit.collider.transform, travelDirection, false);
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

    private void ApplyCombatPolish(Transform hitTransform, Vector3 direction, bool explosionHit)
    {
        if (hitTransform == null)
            return;

        EnemyHitFlash hitFlash =
            hitTransform.GetComponentInParent<EnemyHitFlash>();

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
            float selectedStagger = staggerDuration;
            float selectedKnockback = knockbackStrength;

            if (explosionHit)
            {
                selectedStagger *= explosiveStaggerMultiplier;
                selectedKnockback *= explosiveKnockbackMultiplier;
            }

            reaction.ApplyHitReaction(
                direction,
                selectedKnockback,
                selectedStagger,
                overchargeProjectile
            );
        }

        TriggerHitStop(explosionHit);
    }

    private void TriggerHitStop(bool explosionHit)
    {
        if (!enableHitStop || HitStopManager.Instance == null)
            return;

        if (explosionHit)
        {
            HitStopManager.Instance.DoHitStop(explosiveHitStopDuration);
            return;
        }

        HitStopManager.Instance.DoHitStop(
            overchargeProjectile
                ? overchargeHitStopDuration
                : normalHitStopDuration
        );
    }

    private void TriggerImpactCameraShake()
    {
        if (!enableImpactCameraShake || cameraShake == null)
            return;

        if (explosive)
        {
            cameraShake.Shake(
                explosiveImpactShakeDuration,
                explosiveImpactShakeStrength
            );
        }
        else if (overchargeProjectile)
        {
            cameraShake.Shake(
                overchargeImpactShakeDuration,
                overchargeImpactShakeStrength
            );
        }
        else
        {
            cameraShake.Shake(
                impactShakeDuration,
                impactShakeStrength
            );
        }
    }

    private void SpawnImpactEffects(RaycastHit hit)
    {
        if (impactEffectPrefab != null)
        {
            Instantiate(
                impactEffectPrefab,
                hit.point,
                Quaternion.identity
            );
        }

        if (hitSparkEffectPrefab != null)
        {
            Quaternion sparkRotation =
                Quaternion.LookRotation(hit.normal);

            Instantiate(
                hitSparkEffectPrefab,
                hit.point,
                sparkRotation
            );
        }
    }

    private void Explode(Vector3 centerPoint, Transform directHitEnemyRoot)
    {
        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, centerPoint, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(
            centerPoint,
            explosionRadius,
            hitLayers,
            QueryTriggerInteraction.Ignore
        );

        HashSet<Transform> damagedEnemies = new HashSet<Transform>();

        foreach (Collider col in hits)
        {
            Transform enemyRoot = GetEnemyRoot(col.transform);

            if (enemyRoot == null)
                continue;

            if (damagedEnemies.Contains(enemyRoot))
                continue;

            damagedEnemies.Add(enemyRoot);

            IDamageable damageable = enemyRoot.GetComponent<IDamageable>();

            if (damageable == null)
                damageable = enemyRoot.GetComponentInParent<IDamageable>();

            if (damageable == null || shooterStats == null)
                continue;

            float damageAmount =
                shooterStats.attack *
                damageMultiplier *
                explosionDamageMultiplier;

            if (enemyRoot == directHitEnemyRoot)
                damageAmount = shooterStats.attack * damageMultiplier;

            damageable.TakeDamage(damageAmount, shooterStats);

            Vector3 knockDirection = enemyRoot.position - centerPoint;
            knockDirection.y = 0f;

            if (knockDirection.sqrMagnitude <= 0.001f)
                knockDirection = travelDirection;

            knockDirection.Normalize();

            ApplyCombatPolish(enemyRoot, knockDirection, true);

            if (enemyRoot != null && shooterTargeting != null)
                shooterTargeting.SetTarget(enemyRoot);
        }
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