using UnityEngine;

public class ToxicologistThrusterAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode thrusterKey = KeyCode.LeftShift;

    [Header("Dash")]
    [SerializeField] private float cooldown = 4f;
    [SerializeField] private float containmentCooldown = 1f;
    [SerializeField] private float dashDistance = 7f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Trail")]
    [SerializeField] private GameObject poisonTrailPrefab;
    [SerializeField] private float trailSpawnInterval = 0.035f;
    [SerializeField] private float trailSpawnHeightOffset = 0.1f;
    [SerializeField] private float trailSideOffset = 0.45f;
    [SerializeField] private float containmentTrailSideOffset = 0.75f;
    [SerializeField] private bool spawnSideTrails = true;

    [Header("VFX")]
    [SerializeField] private GameObject startBurstVFXPrefab;
    [SerializeField] private GameObject endBurstVFXPrefab;
    [SerializeField] private Transform thrusterVFXSpawnPoint;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip thrusterSound;

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    private CharacterController characterController;
    private PlayerMovementController movementController;
    private ToxicologistContainmentFailureAbility containmentFailure;

    private float nextUseTime;
    private bool isDashing;
    private float dashTimer;
    private Vector3 dashDirection;
    private float trailTimer;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        movementController = GetComponent<PlayerMovementController>();
        containmentFailure = GetComponent<ToxicologistContainmentFailureAbility>();

        if (abilityHUD == null)
            abilityHUD = FindHUDByName("ToxicologistAbilityHUD");
    }

    private void Update()
    {
        if (Input.GetKeyDown(thrusterKey))
            TryActivateThrusters();

        if (isDashing)
            UpdateDash();
    }

    private void TryActivateThrusters()
    {
        if (isDashing)
            return;

        if (Time.time < nextUseTime)
            return;

        dashDirection = GetDashDirection();

        if (dashDirection.sqrMagnitude <= 0.001f)
            dashDirection = transform.forward;

        dashDirection.y = 0f;
        dashDirection.Normalize();

        StartDash();
    }

    private Vector3 GetDashDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);

        if (inputDirection.sqrMagnitude <= 0.001f)
            return transform.forward;

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

    private void StartDash()
    {
        isDashing = true;
        dashTimer = 0f;
        trailTimer = 0f;

        float currentCooldown = GetCurrentCooldown();
        nextUseTime = Time.time + currentCooldown;

        if (movementController != null)
            movementController.enabled = false;

        SpawnVFX(startBurstVFXPrefab);
        SpawnPoisonTrailCluster();

        if (audioSource != null && thrusterSound != null)
            audioSource.PlayOneShot(thrusterSound);

        if (abilityHUD != null)
            abilityHUD.TriggerMobilityCooldown();
        else
            Debug.LogWarning("[ToxicologistThrusterAbility] Missing Ability HUD reference.");

        AbilityWeaveManager.Instance?.RecordAbilityUsed(
            CombatClassType.Toxicologist,
            AbilitySlotType.Movement
        );

        Debug.Log($"[ToxicologistThrusterAbility] Toxic Thrusters activated. Cooldown={currentCooldown}");
    }

    private void UpdateDash()
    {
        dashTimer += Time.deltaTime;
        trailTimer += Time.deltaTime;

        float normalizedTime = Mathf.Clamp01(dashTimer / dashDuration);
        float curveValue = dashCurve.Evaluate(normalizedTime);

        float previousTime = Mathf.Clamp01((dashTimer - Time.deltaTime) / dashDuration);
        float previousCurveValue = dashCurve.Evaluate(previousTime);

        float curveDelta = curveValue - previousCurveValue;
        Vector3 moveAmount = dashDirection * dashDistance * curveDelta;

        if (characterController != null)
            characterController.Move(moveAmount);
        else
            transform.position += moveAmount;

        if (trailTimer >= trailSpawnInterval)
        {
            trailTimer = 0f;
            SpawnPoisonTrailCluster();
        }

        if (normalizedTime >= 1f)
            EndDash();
    }

    private void EndDash()
    {
        isDashing = false;

        if (movementController != null)
            movementController.enabled = true;

        SpawnPoisonTrailCluster();
        SpawnVFX(endBurstVFXPrefab);

        Debug.Log("[ToxicologistThrusterAbility] Toxic Thrusters ended.");
    }

    private void SpawnPoisonTrailCluster()
    {
        if (poisonTrailPrefab == null)
            return;

        Vector3 basePosition = transform.position;
        basePosition.y += trailSpawnHeightOffset;

        Instantiate(poisonTrailPrefab, basePosition, Quaternion.identity);

        if (!spawnSideTrails)
            return;

        Vector3 right = Vector3.Cross(Vector3.up, dashDirection).normalized;
        float currentSideOffset = GetCurrentTrailSideOffset();

        Instantiate(
            poisonTrailPrefab,
            basePosition + right * currentSideOffset,
            Quaternion.identity
        );

        Instantiate(
            poisonTrailPrefab,
            basePosition - right * currentSideOffset,
            Quaternion.identity
        );
    }

    private void SpawnVFX(GameObject prefab)
    {
        if (prefab == null)
            return;

        Vector3 spawnPosition = transform.position;

        if (thrusterVFXSpawnPoint != null)
            spawnPosition = thrusterVFXSpawnPoint.position;

        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }

    private float GetCurrentCooldown()
    {
        if (IsContainmentFailureActive())
            return containmentCooldown;

        return cooldown;
    }

    private float GetCurrentTrailSideOffset()
    {
        if (IsContainmentFailureActive())
            return containmentTrailSideOffset;

        return trailSideOffset;
    }

    private bool IsContainmentFailureActive()
    {
        return containmentFailure != null &&
               containmentFailure.IsContainmentFailureActive;
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
}