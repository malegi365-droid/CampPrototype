using UnityEngine;
using UnityEngine.InputSystem;

public enum BasicAttackMode
{
    TankSweep,
    DpsStraightShot,
    HealerAimedShot
}

[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(TargetingController))]
public class DirectionalBasicAttackController : MonoBehaviour
{
    [Header("Attack Mode")]
    [SerializeField] private BasicAttackMode attackMode;

    [Header("Input")]
    [SerializeField] private bool holdToAttack = true;
    [SerializeField] private float attackCooldown = 0.35f;

    [Header("Aiming")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private bool faceMouseBeforeAttack = true;

    [Header("Tank Sweep")]
    [SerializeField] private float sweepRange = 2.5f;
    [SerializeField] private float sweepAngle = 110f;

    [Header("DPS Straight Shot")]
    [SerializeField] private float shotRange = 25f;
    [SerializeField] private float shotRadius = 1.25f;

    [Header("Healer Aimed Shot")]
    [SerializeField] private float healerShotRange = 18f;
    [SerializeField] private float healerShotRadius = 0.5f;

    [Header("Detection")]
    [SerializeField] private LayerMask hitLayers = ~0;

    [Header("Visual Debug")]
    [SerializeField] private bool showVisibleTracer = true;
    [SerializeField] private float tracerDuration = 0.08f;

    private UnitStats stats;
    private TargetingController targetingController;
    private float attackTimer;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
        targetingController = GetComponent<TargetingController>();

        if (aimCamera == null)
            aimCamera = Camera.main;
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        bool shouldAttack = holdToAttack
            ? mouse.leftButton.isPressed
            : mouse.leftButton.wasPressedThisFrame;

        if (shouldAttack)
            TryAttack();
    }

    private void TryAttack()
    {
        if (attackTimer > 0f)
            return;

        if (faceMouseBeforeAttack)
            FaceMouseGroundPoint();

        bool attacked = false;

        switch (attackMode)
        {
            case BasicAttackMode.TankSweep:
                attacked = PerformTankSweep();
                break;

            case BasicAttackMode.DpsStraightShot:
                attacked = PerformDpsStraightShot();
                break;

            case BasicAttackMode.HealerAimedShot:
                attacked = PerformHealerAimedShot();
                break;
        }

        attackTimer = attackCooldown;
    }

    private bool PerformTankSweep()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            sweepRange,
            hitLayers,
            QueryTriggerInteraction.Ignore
        );

        bool hitAny = false;

        foreach (Collider hit in hits)
        {
            Transform enemyRoot = GetValidEnemyRoot(hit.transform);
            if (enemyRoot == null)
                continue;

            Vector3 toTarget = enemyRoot.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= 0.001f)
                continue;

            float angle = Vector3.Angle(transform.forward, toTarget.normalized);
            if (angle > sweepAngle * 0.5f)
                continue;

            DealDamage(enemyRoot, stats.attack);
            hitAny = true;
        }

        return hitAny;
    }

    private bool PerformDpsStraightShot()
    {
        Vector3 origin = transform.position + Vector3.up * 1.25f;
        Vector3 direction = transform.forward;
        Vector3 endPoint = origin + direction * shotRange;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            shotRadius,
            direction,
            shotRange,
            hitLayers,
            QueryTriggerInteraction.Ignore
        );

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == transform)
                    continue;

                Transform enemyRoot = GetValidEnemyRoot(hit.transform);
                if (enemyRoot == null)
                    continue;

                endPoint = hit.point;

                if (showVisibleTracer)
                    SpawnTracer(origin, endPoint);

                DealDamage(enemyRoot, stats.attack);
                return true;
            }
        }

        if (showVisibleTracer)
            SpawnTracer(origin, endPoint);

        return false;
    }

    private bool PerformHealerAimedShot()
    {
        Vector3 origin = transform.position + Vector3.up * 1.25f;
        Vector3 direction = transform.forward;
        Vector3 endPoint = origin + direction * healerShotRange;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            healerShotRadius,
            direction,
            healerShotRange,
            hitLayers,
            QueryTriggerInteraction.Ignore
        );

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                Transform enemyRoot = GetValidEnemyRoot(hit.transform);
                if (enemyRoot == null)
                    continue;

                endPoint = hit.point;

                if (showVisibleTracer)
                    SpawnTracer(origin, endPoint);

                DealDamage(enemyRoot, stats.attack);
                return true;
            }
        }

        if (showVisibleTracer)
            SpawnTracer(origin, endPoint);

        return false;
    }

    private Transform GetValidEnemyRoot(Transform candidate)
    {
        if (candidate == null || candidate == transform)
            return null;

        UnitStats stats = candidate.GetComponent<UnitStats>();
        if (stats == null)
            stats = candidate.GetComponentInParent<UnitStats>();

        HealthController health = candidate.GetComponent<HealthController>();
        if (health == null)
            health = candidate.GetComponentInParent<HealthController>();

        if (stats == null || health == null)
            return null;

        if (stats.role != UnitRole.Enemy)
            return null;

        if (health.IsDead())
            return null;

        return stats.transform;
    }

    private void DealDamage(Transform enemyRoot, float damage)
    {
        IDamageable dmg = enemyRoot.GetComponent<IDamageable>();
        if (dmg == null)
            dmg = enemyRoot.GetComponentInParent<IDamageable>();

        if (dmg == null)
            return;

        dmg.TakeDamage(damage, stats);

        ThreatTable threat = enemyRoot.GetComponent<ThreatTable>();
        if (threat != null)
            threat.AddThreat(gameObject, damage * stats.threatMultiplier);

        targetingController.SetTarget(enemyRoot);

        Debug.Log($"{gameObject.name} hit {enemyRoot.name} for {damage}");
    }

    private void FaceMouseGroundPoint()
    {
        if (aimCamera == null)
            return;

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        Ray ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (!groundPlane.Raycast(ray, out float enter))
            return;

        Vector3 point = ray.GetPoint(enter);
        Vector3 direction = point - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        // 🔥 INSTANT ROTATION FIX
        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = new GameObject("Tracer");
        LineRenderer line = tracer.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.startWidth = 0.08f;
        line.endWidth = 0.08f;
        line.material = new Material(Shader.Find("Sprites/Default"));

        Destroy(tracer, tracerDuration);
    }
}