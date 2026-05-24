using UnityEngine;
using UnityEngine.InputSystem;

public class DPSProjectileFireController : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Muzzle Flash")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string fireTriggerName = "Fire";

    [Header("Camera Feedback")]
    [SerializeField] private CameraShakeController cameraShake;
    [SerializeField] private float shakeDuration = 0.06f;
    [SerializeField] private float shakeStrength = 0.04f;

    [Header("Aiming")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform aimDebugMarker;

    [Header("Fire Settings")]
    [SerializeField] private float fireCooldown = 0.25f;

    private float nextFireTime;
    private UnitStats shooterStats;

    private void Awake()
    {
        shooterStats = GetComponent<UnitStats>();

        if (aimCamera == null)
            aimCamera = Camera.main;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            FireAtCursor(mouse.position.ReadValue());
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void FireAtCursor(Vector2 mouseScreenPosition)
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogWarning("[DPSProjectileFireController] Missing projectile prefab or spawn point.");
            return;
        }

        if (characterAnimator != null)
            characterAnimator.SetTrigger(fireTriggerName);

        if (cameraShake != null)
            cameraShake.Shake(shakeDuration, shakeStrength);

        if (aimCamera == null)
            aimCamera = Camera.main;

        Vector3 cursorWorldPoint = projectileSpawnPoint.position + projectileSpawnPoint.forward * 10f;

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

        Vector3 targetPoint = cursorWorldPoint;

        if (aimDebugMarker != null)
            targetPoint = aimDebugMarker.position;

        Vector3 fireDirection = targetPoint - projectileSpawnPoint.position;
        fireDirection.y = 0f;

        if (fireDirection.sqrMagnitude <= 0.001f)
            fireDirection = transform.forward;

        fireDirection.Normalize();

        Quaternion fireRotation = Quaternion.LookRotation(fireDirection, Vector3.up);

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(
                muzzleFlashPrefab,
                projectileSpawnPoint.position,
                fireRotation
            );

            Destroy(flash, 1f);
        }

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            fireRotation
        );

        DPSInjectorProjectile projectile = projectileObject.GetComponent<DPSInjectorProjectile>();
        if (projectile != null)
            projectile.Initialize(fireDirection, shooterStats);
    }
}