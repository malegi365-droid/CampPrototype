using UnityEngine;
using UnityEngine.InputSystem;

public class DPSProjectileFireController : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Aiming")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform aimDebugMarker;

    [Header("Fire Settings")]
    [SerializeField] private float fireCooldown = 0.25f;

    private float nextFireTime;

    private void Awake()
    {
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

        if (aimCamera == null)
            aimCamera = Camera.main;

        Vector3 fireDirection = transform.forward;

        if (aimCamera != null)
        {
            Ray ray = aimCamera.ScreenPointToRay(mouseScreenPosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 cursorWorldPoint = ray.GetPoint(enter);

                if (aimDebugMarker != null)
                    aimDebugMarker.position = cursorWorldPoint;

                fireDirection = cursorWorldPoint - projectileSpawnPoint.position;
                fireDirection.y = 0f;

                if (fireDirection.sqrMagnitude <= 0.001f)
                    fireDirection = transform.forward;
            }
        }

        fireDirection.Normalize();

        Quaternion projectileRotation = Quaternion.LookRotation(fireDirection, Vector3.up);

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            projectileRotation
        );

        DPSInjectorProjectile projectile = projectileObject.GetComponent<DPSInjectorProjectile>();
        if (projectile != null)
            projectile.Initialize(fireDirection);
    }
}