using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianMeteorDiveAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode meteorDiveKey = KeyCode.E;

    [Header("Weave Requirement")]
    [SerializeField] private bool requireMeteorDiveReady = true;

    [Header("Meteor Dive Settings")]
    [SerializeField] private float damage = 55f;
    [SerializeField] private float range = 8f;
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float cooldown = 6f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Timing")]
    [SerializeField] private float impactDelay = 1.05f;
    [SerializeField] private bool lockDuringMeteorDive = true;
    [SerializeField] private float lockDuration = 0.8f;

    [Header("Hit Reaction")]
    [SerializeField] private float knockbackStrength = 1.5f;
    [SerializeField] private float staggerDuration = 0.4f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string meteorDiveTriggerName = "MeteorDive";

    [Header("Camera Feedback")]
    [SerializeField] private CameraShakeController cameraShake;
    [SerializeField] private float shakeDuration = 0.18f;
    [SerializeField] private float shakeStrength = 0.22f;

    [Header("Meteor Dive VFX")]
    [SerializeField] private GameObject meteorDiveVFXPrefab;
    [SerializeField] private float vfxLifetime = 1.5f;
    [SerializeField] private float vfxForwardOffset = 1.5f;

    [Header("Impact VFX")]
    [SerializeField] private GameObject impactVFXPrefab;
    [SerializeField] private Transform impactPoint;
    [SerializeField] private float impactVFXLifetime = 1f;
    [SerializeField] private Vector3 impactVFXOffset = Vector3.zero;

    [Header("Discovery Presentation")]
    [SerializeField] private bool showTechniqueDiscovery = true;
    [SerializeField] private string techniqueDisplayName = "Meteor Dive";
    [SerializeField] private float discoveryDelayAfterImpact = 0.35f;

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    [Header("Debug")]
    [SerializeField] private bool logCooldownBlocked = false;
    [SerializeField] private bool logHits = true;

    private float nextMeteorDiveTime;
    private bool isMeteorDiving;
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
        if (Input.GetKeyDown(meteorDiveKey))
            TryMeteorDive();
    }

    public void ForceMeteorDiveForShowcase()
    {
        TryMeteorDive();
    }

    private void TryMeteorDive()
    {
        if (isMeteorDiving)
            return;

        if (Time.time < nextMeteorDiveTime)
        {
            if (logCooldownBlocked)
            {
                float remaining = nextMeteorDiveTime - Time.time;
                Debug.Log($"[GuardianMeteorDiveAbility] Meteor Dive on cooldown: {remaining:F1}s remaining.");
            }

            return;
        }

        if (requireMeteorDiveReady)
        {
            if (AbilityWeaveManager.Instance == null ||
                !AbilityWeaveManager.Instance.ConsumeMeteorDiveReady())
            {
                Debug.Log("[GuardianMeteorDiveAbility] Meteor Dive blocked: technique not ready.");
                return;
            }
        }

        nextMeteorDiveTime = Time.time + cooldown;

        if (abilityHUD != null)
            abilityHUD.TriggerSignatureCooldown();
        else
            Debug.LogWarning("[GuardianMeteorDiveAbility] Missing Ability HUD reference.");

        StartCoroutine(MeteorDiveRoutine());
    }

    private IEnumerator MeteorDiveRoutine()
    {
        isMeteorDiving = true;

        if (animator != null && !string.IsNullOrWhiteSpace(meteorDiveTriggerName))
            animator.SetTrigger(meteorDiveTriggerName);

        yield return new WaitForSeconds(impactDelay);

        SpawnMeteorDiveVFX();
        SpawnImpactVFX();
        HitEnemies();
        ShakeCamera();

        Debug.Log("[GuardianMeteorDiveAbility] Meteor Dive impact.");

        if (showTechniqueDiscovery)
            StartCoroutine(ShowDiscoveryAfterDelay());

        if (lockDuringMeteorDive)
        {
            float remainingLock = Mathf.Max(0f, lockDuration - impactDelay);
            yield return new WaitForSecondsRealtime(remainingLock);
        }

        isMeteorDiving = false;

        Debug.Log("[GuardianMeteorDiveAbility] Meteor Dive complete.");
    }

    private IEnumerator ShowDiscoveryAfterDelay()
    {
        yield return new WaitForSecondsRealtime(discoveryDelayAfterImpact);

        TechniqueDiscoveryPresentation.Instance?.ShowTechniqueDiscovery(techniqueDisplayName);
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
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (damagedTargets.Contains(damageable))
                continue;

            damagedTargets.Add(damageable);

            damageable.TakeDamage(damage, attackerStats);
            ApplyHitReaction(hit);

            if (logHits)
                Debug.Log($"[GuardianMeteorDiveAbility] Hit {hit.collider.name} for {damage} damage.");
        }
    }

    private void ApplyHitReaction(RaycastHit hit)
    {
        EnemyHitReactionController reaction =
            hit.collider.GetComponentInParent<EnemyHitReactionController>();

        if (reaction == null)
            return;

        Vector3 knockbackDirection = hit.collider.transform.position - transform.position;
        knockbackDirection.y = 0f;

        if (knockbackDirection.sqrMagnitude > 0.001f)
            knockbackDirection.Normalize();
        else
            knockbackDirection = transform.forward;

        reaction.ApplyLaunchReaction(
            knockbackDirection,
            knockbackStrength,
            staggerDuration
        );
    }

    private void SpawnMeteorDiveVFX()
    {
        if (meteorDiveVFXPrefab == null)
            return;

        Vector3 spawnPosition = transform.position + transform.forward * vfxForwardOffset;
        spawnPosition.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(transform.forward, Vector3.up);

        GameObject vfx = Instantiate(meteorDiveVFXPrefab, spawnPosition, rotation);
        Destroy(vfx, vfxLifetime);
    }

    private void SpawnImpactVFX()
    {
        if (impactVFXPrefab == null)
            return;

        Vector3 spawnPosition = impactPoint != null
            ? impactPoint.position + impactVFXOffset
            : transform.position + transform.forward * vfxForwardOffset + impactVFXOffset;

        GameObject impact = Instantiate(impactVFXPrefab, spawnPosition, Quaternion.identity);
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
            FindObjectsByType<RangerAbilityHUDController>(FindObjectsInactive.Include);

        foreach (RangerAbilityHUDController hud in huds)
        {
            if (hud.gameObject.name == hudName)
                return hud;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 origin = transform.position + Vector3.up * 0.6f;
        Vector3 end = origin + transform.forward * range;

        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(end, radius);

        Gizmos.color = Color.red;

        Vector3 impactPosition = impactPoint != null
            ? impactPoint.position + impactVFXOffset
            : transform.position + transform.forward * vfxForwardOffset + impactVFXOffset;

        Gizmos.DrawWireSphere(impactPosition, 0.35f);
    }
}