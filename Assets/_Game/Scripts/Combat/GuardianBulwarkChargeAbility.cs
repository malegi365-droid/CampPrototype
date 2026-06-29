using System.Collections.Generic;
using UnityEngine;

public class GuardianBulwarkChargeAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode chargeKey = KeyCode.LeftShift;

    [Header("Charge")]
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private float chargeDistance = 8f;
    [SerializeField] private float chargeDuration = 0.35f;
    [SerializeField] private AnimationCurve chargeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Impact")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float hitRadius = 1.25f;
    [SerializeField] private float hitForwardOffset = 1.1f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float pushDistance = 2.5f;
    [SerializeField] private float staggerDuration = 0.25f;

    [Header("Facing")]
    [SerializeField] private Transform facingReference;

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    private CharacterController characterController;
    private PlayerMovementController movementController;
    private UnitStats ownerStats;

    private readonly HashSet<IDamageable> hitThisCharge = new HashSet<IDamageable>();

    private float nextUseTime;
    private bool isCharging;
    private float chargeTimer;
    private Vector3 chargeDirection;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        movementController = GetComponent<PlayerMovementController>();
        ownerStats = GetComponent<UnitStats>();

        if (abilityHUD == null)
            abilityHUD = FindHUDByName("GuardianAbilityHUD");
    }

    private void Update()
    {
        if (Input.GetKeyDown(chargeKey))
            TryActivateCharge();

        if (isCharging)
            UpdateCharge();
    }

    private void TryActivateCharge()
    {
        if (isCharging)
            return;

        if (Time.time < nextUseTime)
            return;

        chargeDirection = GetChargeDirection();

        if (chargeDirection.sqrMagnitude <= 0.001f)
            chargeDirection = GetFacingDirection();

        chargeDirection.y = 0f;
        chargeDirection.Normalize();

        StartCharge();
    }

    private Vector3 GetChargeDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);

        if (inputDirection.sqrMagnitude <= 0.001f)
            return GetFacingDirection();

        Camera cam = Camera.main;

        if (cam == null)
            return inputDirection.normalized;

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        return (camForward * vertical + camRight * horizontal).normalized;
    }

    private Vector3 GetFacingDirection()
    {
        Transform reference = facingReference != null ? facingReference : transform;

        Vector3 direction = reference.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            direction = transform.forward;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return Vector3.forward;

        return direction.normalized;
    }

    private void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0f;
        nextUseTime = Time.time + cooldown;
        hitThisCharge.Clear();

        if (movementController != null)
            movementController.enabled = false;

        if (abilityHUD != null)
            abilityHUD.TriggerMobilityCooldown();
        else
            Debug.LogWarning("[GuardianBulwarkChargeAbility] Missing Ability HUD reference.");

        Debug.Log("[GuardianBulwarkChargeAbility] Bulwark Charge started.");
    }

    private void UpdateCharge()
    {
        chargeTimer += Time.deltaTime;

        float normalizedTime = Mathf.Clamp01(chargeTimer / chargeDuration);
        float curveValue = chargeCurve.Evaluate(normalizedTime);

        float previousTime = Mathf.Clamp01((chargeTimer - Time.deltaTime) / chargeDuration);
        float previousCurveValue = chargeCurve.Evaluate(previousTime);

        float curveDelta = curveValue - previousCurveValue;

        Vector3 moveAmount = chargeDirection * chargeDistance * curveDelta;

        if (characterController != null)
            characterController.Move(moveAmount);
        else
            transform.position += moveAmount;

        CheckForEnemyImpacts();

        if (normalizedTime >= 1f)
            EndCharge();
    }

    private void CheckForEnemyImpacts()
    {
        Vector3 center = transform.position + chargeDirection * hitForwardOffset;
        center.y += 1f;

        Collider[] hits = Physics.OverlapSphere(
            center,
            hitRadius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            UnitStats targetStats = hit.GetComponentInParent<UnitStats>();

            if (targetStats == null || targetStats.role != UnitRole.Enemy)
                continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (hitThisCharge.Contains(damageable))
                continue;

            hitThisCharge.Add(damageable);

            damageable.TakeDamage(damage, ownerStats);

            PushEnemy(targetStats.transform);

            EnemyHitReactionController reaction =
                targetStats.GetComponent<EnemyHitReactionController>();

            if (reaction != null)
                reaction.ApplyStagger(staggerDuration);

            Debug.Log($"[GuardianBulwarkChargeAbility] Hit {targetStats.name} for {damage}.");
        }
    }

    private void PushEnemy(Transform enemyTransform)
    {
        Vector3 push = chargeDirection * pushDistance;
        push.y = 0f;

        CharacterController enemyController =
            enemyTransform.GetComponent<CharacterController>();

        if (enemyController != null)
        {
            enemyController.Move(push);
            return;
        }

        enemyTransform.position += push;
    }

    private void EndCharge()
    {
        isCharging = false;

        if (movementController != null)
            movementController.enabled = true;

        Debug.Log("[GuardianBulwarkChargeAbility] Bulwark Charge ended.");
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
        Vector3 direction = chargeDirection.sqrMagnitude > 0.001f
            ? chargeDirection
            : transform.forward;

        Vector3 center = transform.position + direction.normalized * hitForwardOffset;
        center.y += 1f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, hitRadius);
    }
}