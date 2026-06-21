using System.Collections;
using UnityEngine;

public class OverwatchDroneController : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Vector3 followOffset = new Vector3(0.45f, 1.45f, -0.25f);
    [SerializeField] private float followSpeed = 24f;
    [SerializeField] private bool useActivePlayerRotation = true;
    [SerializeField] private bool snapToShoulderOnInitialize = true;

    [Header("Firing")]
    [SerializeField] private float knockbackStrength = 0.25f;
    [SerializeField] private float staggerDuration = 0.08f;

    [Header("Pulse Shot Visual")]
    [SerializeField] private LineRenderer pulseLine;
    [SerializeField] private float pulseDuration = 0.12f;
    [SerializeField] private float pulseWidth = 0.045f;
    [SerializeField] private float pulseLength = 1.2f;
    [SerializeField] private Color pulseColor = new Color(0.2f, 0.9f, 1f, 1f);
    [SerializeField] private Vector3 pulseStartOffset = Vector3.zero;
    [SerializeField] private Vector3 pulseTargetOffset = new Vector3(0f, 0.8f, 0f);

    [Header("Aiming Visual")]
    [SerializeField] private float aimTurnSpeed = 10f;
    [SerializeField] private float returnToPlayerFacingDelay = 1.25f;
    [SerializeField] private bool yawOnly = true;

    [Header("Hover")]
    [SerializeField] private float hoverBobAmount = 0.05f;
    [SerializeField] private float hoverBobSpeed = 2.25f;

    [Header("Safety")]
    [SerializeField] private bool disableOwnColliders = true;
    [SerializeField] private bool disableOwnRigidbodies = true;

    private UnitStats ownerStats;
    private float damageMultiplier;
    private float fireInterval;
    private float targetRange;
    private LayerMask enemyLayer;

    private float endTime;
    private float nextFireTime;
    private float lastFireTime = -999f;
    private float bobSeed;

    private PartyControlManager partyControlManager;
    private Coroutine pulseRoutine;

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
        bobSeed = Random.Range(0f, 100f);

        partyControlManager = FindAnyObjectByType<PartyControlManager>();

        MakeDroneNonCombatant();
        SetupPulseLine();

        if (snapToShoulderOnInitialize)
            SnapToShoulder();

        Destroy(gameObject, duration + 0.25f);

        Debug.Log("[OverwatchDroneController] Overwatch drone initialized.");
    }

    private void LateUpdate()
    {
        FollowActivePlayer();

        if (Time.time >= endTime)
        {
            Destroy(gameObject);
            return;
        }

        Transform enemy = FindNearestEnemy();

        if (enemy != null)
        {
            AimAtEnemy(enemy);

            if (Time.time >= nextFireTime)
            {
                FireAtEnemy(enemy);
                nextFireTime = Time.time + fireInterval;
            }
        }
        else
        {
            ReturnToPlayerFacingAfterDelay();
        }
    }

    private void SnapToShoulder()
    {
        Transform target = GetActivePlayerTransform();

        if (target == null)
            return;

        transform.position = GetShoulderPosition(target);
        transform.rotation = GetPlayerYawRotation(target);
    }

    private void FollowActivePlayer()
    {
        Transform target = GetActivePlayerTransform();

        if (target == null)
            return;

        Vector3 desiredPosition = GetShoulderPosition(target);

        float bob = Mathf.Sin((Time.time + bobSeed) * hoverBobSpeed) * hoverBobAmount;
        desiredPosition += Vector3.up * bob;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * followSpeed
        );
    }

    private Vector3 GetShoulderPosition(Transform target)
    {
        if (useActivePlayerRotation)
            return target.position + target.rotation * followOffset;

        return target.position + followOffset;
    }

    private void AimAtEnemy(Transform enemy)
    {
        Vector3 direction = enemy.position + pulseTargetOffset - transform.position;

        if (yawOnly)
            direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * aimTurnSpeed
        );
    }

    private void ReturnToPlayerFacingAfterDelay()
    {
        if (Time.time < lastFireTime + returnToPlayerFacingDelay)
            return;

        Transform activePlayer = GetActivePlayerTransform();

        if (activePlayer == null)
            return;

        Quaternion targetRotation = GetPlayerYawRotation(activePlayer);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * aimTurnSpeed
        );
    }

    private Quaternion GetPlayerYawRotation(Transform player)
    {
        Vector3 forward = player.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.001f)
            forward = Vector3.forward;

        return Quaternion.LookRotation(forward.normalized, Vector3.up);
    }

    private void FireAtEnemy(Transform enemy)
    {
        if (enemy == null || ownerStats == null)
            return;

        IDamageable damageable = enemy.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = enemy.GetComponentInParent<IDamageable>();

        if (damageable == null)
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

        ShowPulseShot(enemy);
        lastFireTime = Time.time;

        Debug.Log($"[OverwatchDroneController] Fired at {enemy.name} for {damage} damage.");
    }

    private void ShowPulseShot(Transform enemy)
    {
        if (pulseLine == null)
            return;

        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        Vector3 start = transform.position + pulseStartOffset;
        Vector3 end = enemy.position + pulseTargetOffset;

        pulseRoutine = StartCoroutine(PulseShotRoutine(start, end));
    }

    private IEnumerator PulseShotRoutine(Vector3 start, Vector3 end)
    {
        pulseLine.enabled = true;
        pulseLine.positionCount = 2;

        float elapsed = 0f;
        Vector3 shotDirection = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / pulseDuration);
            float centerDistance = Mathf.Lerp(0f, distance, t);

            Vector3 pulseCenter = start + shotDirection * centerDistance;
            Vector3 pulseStart = pulseCenter - shotDirection * (pulseLength * 0.5f);
            Vector3 pulseEnd = pulseCenter + shotDirection * (pulseLength * 0.5f);

            pulseLine.SetPosition(0, pulseStart);
            pulseLine.SetPosition(1, pulseEnd);

            yield return null;
        }

        pulseLine.enabled = false;
        pulseRoutine = null;
    }

    private void SetupPulseLine()
    {
        if (pulseLine == null)
            pulseLine = GetComponent<LineRenderer>();

        if (pulseLine == null)
            pulseLine = gameObject.AddComponent<LineRenderer>();

        pulseLine.enabled = false;
        pulseLine.positionCount = 2;
        pulseLine.startWidth = pulseWidth;
        pulseLine.endWidth = pulseWidth * 0.35f;
        pulseLine.useWorldSpace = true;

        Material pulseMaterial = new Material(Shader.Find("Sprites/Default"));
        pulseMaterial.color = pulseColor;
        pulseLine.material = pulseMaterial;
    }

    private void MakeDroneNonCombatant()
    {
        if (disableOwnColliders)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();

            foreach (Collider col in colliders)
                col.enabled = false;
        }

        if (disableOwnRigidbodies)
        {
            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
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