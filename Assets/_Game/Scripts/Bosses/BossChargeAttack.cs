using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BossChargeAttack : MonoBehaviour
{
    [Header("Charge Settings")]
    [SerializeField] private float chargeCooldown = 5f;
    [SerializeField] private float chargeSpeed = 14f;
    [SerializeField] private float chargeDuration = 1.0f;
    [SerializeField] private float chargeDamage = 30f;
    [SerializeField] private float chargeHitRadius = 1.75f;

    [Header("Telegraph")]
    [SerializeField] private float windupTime = 0.75f;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private LayerMask playerLayers;
    [SerializeField] private float wallCheckDistance = 0.35f;
    [SerializeField] private float wallCheckRadius = 0.65f;
    [SerializeField] private float wallCheckHeight = 0.9f;
    [SerializeField] private float wallStopBuffer = 0.08f;

    [Header("Debug")]
    [SerializeField] private bool logChargeEvents = true;

    private EnemyAIController enemyAI;
    private AutoAttackController autoAttack;
    private Rigidbody rb;

    private bool isCharging = false;
    private bool hasHitPlayerThisCharge = false;
    private float lastChargeTime;

    private Transform player;
    private Vector3 lockedChargeDirection;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIController>();
        autoAttack = GetComponent<AutoAttackController>();
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (isCharging)
            return;

        FindCurrentPlayer();

        if (player == null)
            return;

        if (Time.time - lastChargeTime >= chargeCooldown)
        {
            StartCoroutine(ChargeRoutine());
        }
    }

    private void FindCurrentPlayer()
    {
        PartyMemberControlBridge[] members = FindObjectsOfType<PartyMemberControlBridge>();

        foreach (PartyMemberControlBridge member in members)
        {
            if (member != null && member.IsPlayerControlled)
            {
                player = member.transform;
                return;
            }
        }

        player = null;
    }

    private IEnumerator ChargeRoutine()
    {
        isCharging = true;
        hasHitPlayerThisCharge = false;
        lastChargeTime = Time.time;

        if (enemyAI != null)
            enemyAI.enabled = false;

        if (autoAttack != null)
            autoAttack.SetTarget(null);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.01f)
        {
            EndCharge();
            yield break;
        }

        lockedChargeDirection = dir.normalized;
        transform.rotation = Quaternion.LookRotation(lockedChargeDirection, Vector3.up);

        if (logChargeEvents)
            Debug.Log("[BossChargeAttack] Telegraphing charge.");

        yield return new WaitForSeconds(windupTime);

        float timer = 0f;

        while (timer < chargeDuration)
        {
            CheckPlayerHit();

            float moveDistance = chargeSpeed * Time.deltaTime;

            if (WouldHitWall(moveDistance, out RaycastHit wallHit))
            {
                float safeDistance = Mathf.Max(0f, wallHit.distance - wallStopBuffer);

                if (safeDistance > 0f)
                    MoveBoss(safeDistance);

                if (logChargeEvents)
                    Debug.Log($"[BossChargeAttack] Charge stopped by wall: {wallHit.collider.name}");

                break;
            }

            MoveBoss(moveDistance);

            CheckPlayerHit();

            timer += Time.deltaTime;
            yield return null;
        }

        EndCharge();
    }

    private void MoveBoss(float distance)
    {
        Vector3 nextPosition = rb.position + lockedChargeDirection * distance;
        rb.MovePosition(nextPosition);
    }

    private void EndCharge()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isCharging = false;

        if (enemyAI != null)
            enemyAI.enabled = true;

        if (logChargeEvents)
            Debug.Log("[BossChargeAttack] Charge ended.");
    }

    private bool WouldHitWall(float moveDistance, out RaycastHit closestWallHit)
    {
        closestWallHit = new RaycastHit();

        Vector3 origin = transform.position + Vector3.up * wallCheckHeight;
        float checkDistance = moveDistance + wallCheckDistance;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            wallCheckRadius,
            lockedChargeDirection,
            checkDistance,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );

        bool foundWall = false;
        float closestDistance = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                continue;

            UnitStats hitStats = hit.collider.GetComponent<UnitStats>();
            if (hitStats == null)
                hitStats = hit.collider.GetComponentInParent<UnitStats>();

            if (hitStats != null)
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestWallHit = hit;
                foundWall = true;
            }
        }

        return foundWall;
    }

    private void CheckPlayerHit()
    {
        if (hasHitPlayerThisCharge)
            return;

        Vector3 center = transform.position + Vector3.up * wallCheckHeight;

        Collider[] hits = Physics.OverlapSphere(
            center,
            chargeHitRadius,
            playerLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            HealthController health = hit.GetComponent<HealthController>();
            if (health == null)
                health = hit.GetComponentInParent<HealthController>();

            if (health == null || health.IsDead())
                continue;

            UnitStats hitStats = hit.GetComponent<UnitStats>();
            if (hitStats == null)
                hitStats = hit.GetComponentInParent<UnitStats>();

            if (hitStats == null)
                continue;

            if (hitStats.role == UnitRole.Enemy)
                continue;

            health.TakeDamage(chargeDamage);
            hasHitPlayerThisCharge = true;

            if (logChargeEvents)
                Debug.Log("[BossChargeAttack] Boss charge hit player.");

            return;
        }
    }
}