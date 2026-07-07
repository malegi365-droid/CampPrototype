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

    [Header("Volatile Reaction")]
    [SerializeField] private float reactionDamage = 75f;
    [SerializeField] private float reactionRadius = 5f;
    [SerializeField] private float reactionDelay = 0.08f;
    [SerializeField] private GameObject reactionVFXPrefab;
    [SerializeField] private float reactionVFXLifetime = 2f;
    [SerializeField] private bool destroyCloudAfterReaction = true;

    [Header("Discovery")]
    [SerializeField] private bool showDiscovery = true;
    [SerializeField] private string discoveryName = "Volatile Reaction";

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private readonly HashSet<PoisonStatusEffect> poisonedEnemies = new();

    private bool hasReacted;
    private Coroutine cloudRoutine;

    public bool HasReacted => hasReacted;

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
        yield return new WaitForSeconds(reactionDelay);

        if (reactionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(
                reactionVFXPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(vfx, reactionVFXLifetime);
        }

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            reactionRadius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            damageable.TakeDamage(reactionDamage, null);
        }

        if (showDiscovery)
            TechniqueDiscoveryPresentation.Instance?.ShowTechniqueDiscovery(discoveryName);

        if (destroyCloudAfterReaction)
            Destroy(gameObject);
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

            poison.ApplyPoison(poisonDuration, poisonDamagePerTick);

            poisonedEnemies.Add(poison);
        }
    }

    private float GetCloudRadius()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();

        if (sphere != null)
            return sphere.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);

        return 3f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GetCloudRadius());

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, reactionRadius);
    }
}