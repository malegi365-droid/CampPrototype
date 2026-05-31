using UnityEngine;
using UnityEngine.InputSystem;

public class DPSProjectileFireController : MonoBehaviour
{
    [Header("Basic Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Ability Projectiles")]
    [SerializeField] private GameObject piercingProjectilePrefab;
    [SerializeField] private GameObject explosiveProjectilePrefab;
    [SerializeField] private GameObject overchargeProjectilePrefab;

    [Header("Muzzle Flash")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource fireAudioSource;
    [SerializeField] private AudioClip piercingFireSound;
    [SerializeField] private AudioClip explosiveFireSound;
    [SerializeField] private AudioClip overchargeFireSound;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string fireTriggerName = "Fire";

    [Header("HUD")]
    [SerializeField] private DPSAbilityHUDController abilityHUD;

    [Header("Overcharge Visuals")]
    [SerializeField] private OverchargeVisualController overchargeVisuals;

    [Header("Camera Feedback")]
    [SerializeField] private CameraShakeController cameraShake;

    [SerializeField] private float shakeDuration = 0.06f;
    [SerializeField] private float shakeStrength = 0.04f;

    [SerializeField] private float piercingShakeDuration = 0.08f;
    [SerializeField] private float piercingShakeStrength = 0.06f;

    [SerializeField] private float explosiveShakeDuration = 0.12f;
    [SerializeField] private float explosiveShakeStrength = 0.10f;

    [SerializeField] private float overchargeShakeDuration = 0.05f;
    [SerializeField] private float overchargeShakeStrength = 0.07f;

    [Header("Aiming")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform aimDebugMarker;

    [Header("Fire Settings")]
    [SerializeField] private float fireCooldown = 0.25f;

    [Header("Ability Cooldowns")]
    [SerializeField] private float piercingCooldown = 3f;
    [SerializeField] private float explosiveCooldown = 5f;

    [Header("Overcharge")]
    [SerializeField] private bool allowOvercharge = true;
    [SerializeField] private float overchargeDuration = 6f;
    [SerializeField] private float overchargeCooldown = 12f;
    [SerializeField] private float overchargeFireRate = 0.08f;

    private float nextFireTime;
    private float nextPiercingTime;
    private float nextExplosiveTime;

    private float nextOverchargeTime;
    private float overchargeEndTime;

    private bool overchargeActive = false;

    private UnitStats shooterStats;
    private TargetingController shooterTargeting;

    private void Awake()
    {
        shooterStats = GetComponent<UnitStats>();
        shooterTargeting = GetComponent<TargetingController>();

        if (fireAudioSource == null)
            fireAudioSource = GetComponent<AudioSource>();

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (abilityHUD == null)
            abilityHUD = FindAnyObjectByType<DPSAbilityHUDController>();

        if (overchargeVisuals == null)
            overchargeVisuals = GetComponent<OverchargeVisualController>();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        if (mouse == null)
            return;

        Vector2 mousePosition = mouse.position.ReadValue();

        HandleOverchargeState(keyboard);

        if (overchargeActive)
        {
            if (mouse.leftButton.isPressed &&
                Time.time >= nextFireTime)
            {
                GameObject selectedOverchargeProjectile =
                    overchargeProjectilePrefab != null
                        ? overchargeProjectilePrefab
                        : piercingProjectilePrefab;

                FireProjectile(
                    selectedOverchargeProjectile,
                    mousePosition,
                    overchargeFireSound != null
                        ? overchargeFireSound
                        : piercingFireSound,
                    overchargeShakeDuration,
                    overchargeShakeStrength
                );

                nextFireTime = Time.time + overchargeFireRate;
            }

            return;
        }

        if (mouse.leftButton.wasPressedThisFrame &&
            Time.time >= nextFireTime)
        {
            FireProjectile(
                projectilePrefab,
                mousePosition,
                fireAudioSource != null ? fireAudioSource.clip : null,
                shakeDuration,
                shakeStrength
            );

            nextFireTime = Time.time + fireCooldown;
        }

        if (keyboard != null &&
            keyboard.qKey.wasPressedThisFrame &&
            Time.time >= nextPiercingTime)
        {
            FireProjectile(
                piercingProjectilePrefab,
                mousePosition,
                piercingFireSound != null
                    ? piercingFireSound
                    : fireAudioSource != null
                        ? fireAudioSource.clip
                        : null,
                piercingShakeDuration,
                piercingShakeStrength
            );

            nextPiercingTime = Time.time + piercingCooldown;

            if (abilityHUD != null)
                abilityHUD.TriggerPiercingCooldown();
        }

        if (keyboard != null &&
            keyboard.eKey.wasPressedThisFrame &&
            Time.time >= nextExplosiveTime)
        {
            FireProjectile(
                explosiveProjectilePrefab,
                mousePosition,
                explosiveFireSound != null
                    ? explosiveFireSound
                    : fireAudioSource != null
                        ? fireAudioSource.clip
                        : null,
                explosiveShakeDuration,
                explosiveShakeStrength
            );

            nextExplosiveTime = Time.time + explosiveCooldown;

            if (abilityHUD != null)
                abilityHUD.TriggerExplosiveCooldown();
        }
    }

    private void HandleOverchargeState(Keyboard keyboard)
    {
        if (!allowOvercharge)
            return;

        if (overchargeActive &&
            Time.time >= overchargeEndTime)
        {
            overchargeActive = false;

            if (abilityHUD != null)
                abilityHUD.SetOverchargeState(false);

            if (overchargeVisuals != null)
                overchargeVisuals.DisableOverchargeVisuals();

            Debug.Log("Overcharge ended.");
        }

        if (keyboard == null)
            return;

        if (keyboard.rKey.wasPressedThisFrame &&
            Time.time >= nextOverchargeTime)
        {
            ActivateOvercharge();
        }
    }

    private void ActivateOvercharge()
    {
        overchargeActive = true;

        overchargeEndTime = Time.time + overchargeDuration;
        nextOverchargeTime = Time.time + overchargeCooldown;

        if (abilityHUD != null)
        {
            abilityHUD.TriggerOverchargeCooldown();
            abilityHUD.SetOverchargeState(true);
        }

        if (overchargeVisuals != null)
            overchargeVisuals.EnableOverchargeVisuals();

        Debug.Log("Overcharge activated.");
    }

    public bool IsOverchargeActive()
    {
        return overchargeActive;
    }

    private void FireProjectile(
        GameObject selectedProjectilePrefab,
        Vector2 mouseScreenPosition,
        AudioClip fireSound,
        float selectedShakeDuration,
        float selectedShakeStrength
    )
    {
        if (selectedProjectilePrefab == null ||
            projectileSpawnPoint == null)
        {
            Debug.LogWarning("[DPSProjectileFireController] Missing projectile prefab or spawn point.");
            return;
        }

        if (characterAnimator != null)
            characterAnimator.SetTrigger(fireTriggerName);

        if (fireAudioSource != null && fireSound != null)
            fireAudioSource.PlayOneShot(fireSound);

        if (cameraShake != null)
            cameraShake.Shake(selectedShakeDuration, selectedShakeStrength);

        Vector3 targetPoint = GetAimPoint(mouseScreenPosition);

        Vector3 fireDirection = targetPoint - projectileSpawnPoint.position;
        fireDirection.y = 0f;

        if (fireDirection.sqrMagnitude <= 0.001f)
            fireDirection = transform.forward;

        fireDirection.Normalize();

        Quaternion fireRotation = Quaternion.LookRotation(fireDirection, Vector3.up);

        SpawnMuzzleFlash(fireRotation);

        GameObject projectileObject = Instantiate(
            selectedProjectilePrefab,
            projectileSpawnPoint.position,
            fireRotation
        );

        DPSInjectorProjectile projectile =
            projectileObject.GetComponent<DPSInjectorProjectile>();

        if (projectile != null)
            projectile.Initialize(fireDirection, shooterStats, shooterTargeting);
    }

    private Vector3 GetAimPoint(Vector2 mouseScreenPosition)
    {
        if (aimCamera == null)
            aimCamera = Camera.main;

        Vector3 cursorWorldPoint =
            projectileSpawnPoint.position +
            projectileSpawnPoint.forward * 10f;

        if (aimCamera != null)
        {
            Ray ray = aimCamera.ScreenPointToRay(mouseScreenPosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float enter))
            {
                cursorWorldPoint = ray.GetPoint(enter);

                if (aimDebugMarker != null)
                    aimDebugMarker.position = cursorWorldPoint;
            }
        }

        if (aimDebugMarker != null)
            return aimDebugMarker.position;

        return cursorWorldPoint;
    }

    private void SpawnMuzzleFlash(Quaternion fireRotation)
    {
        if (muzzleFlashPrefab == null)
            return;

        GameObject flash = Instantiate(
            muzzleFlashPrefab,
            projectileSpawnPoint.position,
            fireRotation
        );

        Destroy(flash, 1f);
    }
}