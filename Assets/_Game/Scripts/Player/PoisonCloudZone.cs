using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonCloudZone : MonoBehaviour
{
    [Header("Cloud Settings")]
    [SerializeField] private float cloudDuration = 6f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float poisonDuration = 4f;
    [SerializeField] private float poisonDamagePerTick = 5f;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Cloud Particle Systems")]
    [Tooltip("The normal looping poison mist particle system.")]
    [SerializeField] private ParticleSystem poisonMist;

    [Tooltip("The normal looping poison sparks particle system.")]
    [SerializeField] private ParticleSystem poisonSparks;

    [Tooltip("The normal looping ground-ring particle system.")]
    [SerializeField] private ParticleSystem poisonGroundRing;

    [Tooltip(
        "The non-looping inward-collapse particle system. " +
        "Play On Awake should be disabled."
    )]
    [SerializeField] private ParticleSystem poisonImplosion;

    [Tooltip(
        "When enabled, the normal cloud particles are cleared immediately " +
        "when the implosion begins."
    )]
    [SerializeField] private bool clearNormalCloudParticles = true;

    [Header("Volatile Reaction")]
    [SerializeField] private float reactionDamage = 75f;
    [SerializeField] private float reactionRadius = 5f;

    [Tooltip(
        "Time given to the implosion before the explosion, damage, " +
        "and enemy launch occur."
    )]
    [SerializeField] private float reactionDelay = 0.35f;

    [SerializeField] private GameObject reactionVFXPrefab;
    [SerializeField] private float reactionVFXLifetime = 2f;
    [SerializeField] private Vector3 reactionVFXOffset = Vector3.zero;
    [SerializeField] private bool destroyCloudAfterReaction = true;

    [Header("Enemy Launch")]
    [SerializeField] private float reactionKnockbackStrength = 2f;
    [SerializeField] private float reactionStaggerDuration = 0.45f;

    [Tooltip(
        "Adds upward influence to the radial launch direction. " +
        "Higher values make enemies lift more."
    )]
    [SerializeField] private float upwardLaunchBias = 0.2f;

    [Header("Discovery")]
    [SerializeField] private bool showDiscovery = true;
    [SerializeField] private string discoveryName = "Volatile Reaction";

    [Tooltip(
        "Allows the explosion and enemy launch to play at full speed " +
        "before the discovery presentation begins."
    )]
    [SerializeField] private float discoveryDelayAfterExplosion = 0.4f;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private readonly HashSet<PoisonStatusEffect> poisonedEnemies = new();

    private bool hasReacted;
    private Coroutine cloudRoutine;

    public bool HasReacted => hasReacted;

    private void Awake()
    {
        AutoFindParticleReferences();

        if (poisonImplosion != null)
        {
            poisonImplosion.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    public void Initialize(
        float duration,
        float interval,
        float poisonTime,
        float damagePerTick,
        LayerMask enemies
    )
    {
        cloudDuration = duration;
        tickInterval = interval;
        poisonDuration = poisonTime;
        poisonDamagePerTick = damagePerTick;
        enemyLayer = enemies;

        hasReacted = false;

        AutoFindParticleReferences();
        ResetCloudParticles();

        if (cloudRoutine != null)
            StopCoroutine(cloudRoutine);

        cloudRoutine = StartCoroutine(CloudRoutine());

        Destroy(gameObject, cloudDuration + 0.25f);
    }

    public void TriggerVolatileReaction()
    {
        if (hasReacted)
            return;

        hasReacted = true;

        if (cloudRoutine != null)
        {
            StopCoroutine(cloudRoutine);
            cloudRoutine = null;
        }

        if (logDebug)
            Debug.Log("[PoisonCloudZone] Volatile Reaction triggered.");

        StartCoroutine(VolatileReactionRoutine());
    }

    private IEnumerator CloudRoutine()
    {
        float elapsed = 0f;

        while (elapsed < cloudDuration && !hasReacted)
        {
            ApplyPoisonToEnemiesInside();

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        cloudRoutine = null;
    }

    private IEnumerator VolatileReactionRoutine()
    {
        Vector3 reactionCenter = transform.position;

        StopNormalCloudParticles();
        PlayImplosion();

        if (reactionDelay > 0f)
            yield return new WaitForSeconds(reactionDelay);

        if (reactionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(
                reactionVFXPrefab,
                reactionCenter + reactionVFXOffset,
                Quaternion.identity
            );

            Destroy(vfx, reactionVFXLifetime);
        }

        ApplyVolatileReactionToEnemies(reactionCenter);

        if (showDiscovery)
        {
            yield return new WaitForSecondsRealtime(
                discoveryDelayAfterExplosion
            );

            TechniqueDiscoveryPresentation.Instance
                ?.ShowTechniqueDiscovery(discoveryName);
        }

        if (destroyCloudAfterReaction)
            Destroy(gameObject);
    }

    private void StopNormalCloudParticles()
    {
        ParticleSystemStopBehavior stopBehavior =
            clearNormalCloudParticles
                ? ParticleSystemStopBehavior.StopEmittingAndClear
                : ParticleSystemStopBehavior.StopEmitting;

        StopParticleSystem(poisonMist, stopBehavior);
        StopParticleSystem(poisonSparks, stopBehavior);
        StopParticleSystem(poisonGroundRing, stopBehavior);
    }

    private void StopParticleSystem(
        ParticleSystem particleSystem,
        ParticleSystemStopBehavior stopBehavior
    )
    {
        if (particleSystem == null)
            return;

        particleSystem.Stop(
            true,
            stopBehavior
        );
    }

    private void PlayImplosion()
    {
        if (poisonImplosion == null)
        {
            if (logDebug)
            {
                Debug.LogWarning(
                    "[PoisonCloudZone] Missing PoisonImplosion particle system."
                );
            }

            return;
        }

        poisonImplosion.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        poisonImplosion.Play(true);
    }

    private void ResetCloudParticles()
    {
        RestartParticleSystem(poisonMist);
        RestartParticleSystem(poisonSparks);
        RestartParticleSystem(poisonGroundRing);

        if (poisonImplosion != null)
        {
            poisonImplosion.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    private void RestartParticleSystem(
        ParticleSystem particleSystem
    )
    {
        if (particleSystem == null)
            return;

        particleSystem.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        particleSystem.Play(true);
    }

    private void AutoFindParticleReferences()
    {
        ParticleSystem[] particleSystems =
            GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem == null)
                continue;

            string objectName =
                particleSystem.gameObject.name;

            if (poisonMist == null &&
                objectName == "PoisonMist")
            {
                poisonMist = particleSystem;
            }
            else if (poisonSparks == null &&
                     objectName == "PoisonSparks")
            {
                poisonSparks = particleSystem;
            }
            else if (poisonGroundRing == null &&
                     objectName == "PoisonGroundRing")
            {
                poisonGroundRing = particleSystem;
            }
            else if (poisonImplosion == null &&
                     objectName == "PoisonImplosion")
            {
                poisonImplosion = particleSystem;
            }
        }
    }

    private void ApplyVolatileReactionToEnemies(
        Vector3 reactionCenter
    )
    {
        Collider[] hits = Physics.OverlapSphere(
            reactionCenter,
            reactionRadius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        HashSet<IDamageable> damagedTargets =
            new HashSet<IDamageable>();

        HashSet<EnemyHitReactionController> reactedTargets =
            new HashSet<EnemyHitReactionController>();

        foreach (Collider hit in hits)
        {
            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable != null &&
                damagedTargets.Add(damageable))
            {
                damageable.TakeDamage(
                    reactionDamage,
                    null
                );
            }

            EnemyHitReactionController reaction =
                hit.GetComponentInParent<
                    EnemyHitReactionController
                >();

            if (reaction == null ||
                !reactedTargets.Add(reaction))
            {
                continue;
            }

            Vector3 enemyPosition =
                reaction.transform.position;

            Vector3 launchDirection =
                enemyPosition - reactionCenter;

            launchDirection.y = 0f;

            if (launchDirection.sqrMagnitude <= 0.001f)
            {
                launchDirection =
                    Random.insideUnitSphere;

                launchDirection.y = 0f;
            }

            launchDirection.Normalize();
            launchDirection.y = upwardLaunchBias;
            launchDirection.Normalize();

            reaction.ApplyLaunchReaction(
                launchDirection,
                reactionKnockbackStrength,
                reactionStaggerDuration
            );

            if (logDebug)
            {
                Debug.Log(
                    "[PoisonCloudZone] Launched enemy " +
                    $"{reaction.gameObject.name} " +
                    $"Direction={launchDirection}, " +
                    $"Strength={reactionKnockbackStrength}."
                );
            }
        }
    }

    private void ApplyPoisonToEnemiesInside()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            GetCloudRadius(),
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            PoisonStatusEffect poison =
                hit.GetComponentInParent<PoisonStatusEffect>();

            if (poison == null)
                continue;

            poison.ApplyPoison(
                poisonDuration,
                poisonDamagePerTick
            );

            poisonedEnemies.Add(poison);
        }
    }

    private float GetCloudRadius()
    {
        SphereCollider sphere =
            GetComponent<SphereCollider>();

        if (sphere != null)
        {
            return sphere.radius *
                   Mathf.Max(
                       transform.localScale.x,
                       transform.localScale.z
                   );
        }

        return 3f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            GetCloudRadius()
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            reactionRadius
        );
    }
}