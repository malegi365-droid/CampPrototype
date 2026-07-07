using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianSunderAxeAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode sunderKey = KeyCode.E;

    [Header("Sunder Settings")]
    [SerializeField] private float damage = 35f;
    [SerializeField] private float range = 8f;
    [SerializeField] private float radius = 1.2f;
    [SerializeField] private float cooldown = 6f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Timing")]
    [SerializeField] private float impactDelay = 0.35f;
    [SerializeField] private bool lockDuringSunder = false;
    [SerializeField] private float lockDuration = 0.55f;

    [Header("Hit Reaction")]
    [SerializeField] private float knockbackStrength = 0.8f;
    [SerializeField] private float staggerDuration = 0.2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string sunderTriggerName = "Sunder";

    [Header("Camera Feedback")]
    [SerializeField] private CameraShakeController cameraShake;
    [SerializeField] private float shakeDuration = 0.12f;
    [SerializeField] private float shakeStrength = 0.12f;

    [Header("Sunder VFX")]
    [SerializeField] private GameObject sunderVFXPrefab;
    [SerializeField] private float vfxLifetime = 1.5f;
    [SerializeField] private float vfxForwardOffset = 1.2f;

    [Header("Impact VFX")]
    [SerializeField] private GameObject impactVFXPrefab;
    [SerializeField] private float impactVFXLifetime = 1f;
    [SerializeField] private Vector3 impactVFXOffset = new Vector3(0f, 0.4f, 0f);

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    [Header("Debug")]
    [SerializeField] private bool logCooldownBlocked = false;
    [SerializeField] private bool logHits = true;

    private float nextSunderTime;
    private bool isSundering;
    private UnitStats attackerStats;

    private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

    private void Awake()
    {
        attackerStats = GetComponent<UnitStats>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (cameraShake == null)
            cameraShake = FindAnyObjectByType<CameraShakeController>();

        if (abilityHUD == null)
            abilityHUD = FindHUDByName("GuardianAbilityHUD");
    }

    private void Update()
    {
        if (Input.GetKeyDown(sunderKey))
            TrySunder();
    }

    private void TrySunder()
    {
        if (isSundering)
            return;

        if (Time.time < nextSunderTime)
        {
            if (logCooldownBlocked)
            {
                float remaining = nextSunderTime - Time.time;
                Debug.Log($"[GuardianSunderAxeAbility] Sunder Axe on cooldown: {remaining:F1}s remaining.");
            }

            return;
        }

        nextSunderTime = Time.time + cooldown;

        if (abilityHUD != null)
            abilityHUD.TriggerSignatureCooldown();
        else
            Debug.LogWarning("[GuardianSunderAxeAbility] Missing Ability HUD reference.");

        AbilityWeaveManager.Instance?.RecordAbilityUsed(
            CombatClassType.Guardian,
            AbilitySlotType.Signature
        );

        StartCoroutine(SunderRoutine());
    }

    private IEnumerator SunderRoutine()
    {
        isSundering = true;

        if (animator != null && !string.IsNullOrWhiteSpace(sunderTriggerName))
            animator.SetTrigger(sunderTriggerName);

        yield return new WaitForSeconds(impactDelay);

        SpawnSunderVFX();
        ShakeCamera();
        HitEnemies();

        Debug.Log("[GuardianSunderAxeAbility] Sunder Axe impact.");

        if (lockDuringSunder)
        {
            float remainingLock = Mathf.Max(0f, lockDuration - impactDelay);
            yield return new WaitForSeconds(remainingLock);
        }

        isSundering = false;

        Debug.Log("[GuardianSunderAxeAbility] Sunder Axe used.");
    }

    private void HitEnemies()
    {
        damagedTargets.Clear();

        Vector3 origin = transform.position + Vector3.up * 0.6f;
        Vector3 direction = transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            radius,
            direction,
            range,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (RaycastHit hit in hits)
        {
            IDamageable damageable =
                hit.collider.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (damagedTargets.Contains(damageable))
                continue;

            damagedTargets.Add(damageable);

            damageable.TakeDamage(damage, attackerStats);
            ApplyHitReaction(hit);
            SpawnImpactVFX(hit);

            if (logHits)
                Debug.Log($"[GuardianSunderAxeAbility] Hit {hit.collider.name} for {damage} damage.");
        }
    }

    private void ApplyHitReaction(RaycastHit hit)
    {
        EnemyHitReactionController reaction =
            hit.collider.GetComponentInParent<EnemyHitReactionController>();

        if (reaction == null)
            return;

        Vector3 knockbackDirection =
            hit.collider.transform.position - transform.position;

        knockbackDirection.y = 0f;

        if (knockbackDirection.sqrMagnitude > 0.001f)
            knockbackDirection.Normalize();
        else
            knockbackDirection = transform.forward;

        reaction.ApplyHitReaction(
            knockbackDirection,
            knockbackStrength,
            staggerDuration
        );
    }

    private void SpawnSunderVFX()
    {
        if (sunderVFXPrefab == null)
            return;

        Vector3 spawnPosition = transform.position + transform.forward * vfxForwardOffset;
        spawnPosition.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(transform.forward, Vector3.up);

        GameObject vfx = Instantiate(
            sunderVFXPrefab,
            spawnPosition,
            rotation
        );

        Destroy(vfx, vfxLifetime);
    }

    private void SpawnImpactVFX(RaycastHit hit)
    {
        if (impactVFXPrefab == null)
            return;

        Vector3 spawnPosition = hit.point + impactVFXOffset;

        GameObject impact = Instantiate(
            impactVFXPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Destroy(impact, impactVFXLifetime);
    }

    private void ShakeCamera()
    {
        if (cameraShake == null)
            return;

        cameraShake.Shake(shakeDuration, shakeStrength);
    }

    private RangerAbilityHUDController FindHUDByName(string hudName)
    {
        RangerAbilityHUDController[] huds =
            FindObjectsByType<RangerAbilityHUDController>(
                FindObjectsInactive.Include
            );

        foreach (RangerAbilityHUDController hud in huds)
        {
            if (hud.gameObject.name == hudName)
                return hud;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position + Vector3.up * 0.6f;
        Vector3 end = origin + transform.forward * range;

        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(end, radius);
    }
}