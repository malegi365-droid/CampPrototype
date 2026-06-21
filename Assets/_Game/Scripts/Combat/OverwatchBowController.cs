using UnityEngine;

public class OverwatchBowController : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Vector3 followOffset = new Vector3(0.8f, 2.1f, 0f);
    [SerializeField] private float followSpeed = 12f;

    [Header("Firing")]
    [SerializeField] private float knockbackStrength = 0.4f;
    [SerializeField] private float staggerDuration = 0.12f;

    [Header("Visual Debug")]
    [SerializeField] private bool faceTarget = true;

    private UnitStats ownerStats;
    private float damageMultiplier;
    private float fireInterval;
    private float targetRange;
    private LayerMask enemyLayer;

    private float endTime;
    private float nextFireTime;

    private PartyControlManager partyControlManager;

    public void Initialize(
        UnitStats stats,
        float duration,
        float multiplier,
        float interval,
        float range,
        LayerMask enemies
    )
    {
        ownerStats = stats;
        damageMultiplier = multiplier;
        fireInterval = Mathf.Max(0.1f, interval);
        targetRange = range;
        enemyLayer = enemies;

        endTime = Time.time + duration;
        nextFireTime = Time.time + 0.25f;

        partyControlManager = FindAnyObjectByType<PartyControlManager>();

        Destroy(gameObject, duration + 0.25f);

        Debug.Log("[OverwatchBowController] Overwatch bow initialized.");
    }

    private void Update()
    {
        FollowActivePlayer();

        if (Time.time >= endTime)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time >= nextFireTime)
        {
            FireAtNearestEnemy();
            nextFireTime = Time.time + fireInterval;
        }
    }

    private void FollowActivePlayer()
    {
        Transform target = GetActivePlayerTransform();

        if (target == null)
            return;

        Vector3 desiredPosition = target.position + followOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * followSpeed
        );
    }

    private void FireAtNearestEnemy()
    {
        Transform enemy = FindNearestEnemy();

        if (enemy == null)
            return;

        if (faceTarget)
        {
            Vector3 lookDirection = enemy.position - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        IDamageable damageable = enemy.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = enemy.GetComponentInParent<IDamageable>();

        if (damageable == null || ownerStats == null)
            return;

        float damage = ownerStats.attack * damageMultiplier;

        damageable.TakeDamage(damage, ownerStats);

        EnemyHitReactionController reaction =
            enemy.GetComponentInParent<EnemyHitReactionController>();

        if (reaction != null)
        {
            Vector3 knockDirection = enemy.position - transform.position;
            knockDirection.y = 0f;

            if (knockDirection.sqrMagnitude > 0.001f)
                knockDirection.Normalize();
            else
                knockDirection = transform.forward;

            reaction.ApplyHitReaction(
                knockDirection,
                knockbackStrength,
                staggerDuration
            );
        }

        Debug.Log($"[OverwatchBowController] Fired at {enemy.name} for {damage} damage.");
    }

    private Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            targetRange,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        Transform nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            UnitStats stats = hit.GetComponentInParent<UnitStats>();

            if (stats == null || stats.role != UnitRole.Enemy)
                continue;

            float distance = Vector3.Distance(transform.position, stats.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = stats.transform;
            }
        }

        return nearest;
    }

    private Transform GetActivePlayerTransform()
    {
        if (partyControlManager == null)
            partyControlManager = FindAnyObjectByType<PartyControlManager>();

        if (partyControlManager == null || partyControlManager.CurrentMember == null)
            return null;

        return partyControlManager.CurrentMember.transform;
    }
}