using UnityEngine;

public class DPSInjectorProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;

    [Header("Hit Detection")]
    [SerializeField] private float hitRadius = 0.2f;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private float armTime = 0.1f;

    [Header("Impact")]
    [SerializeField] private GameObject impactEffectPrefab;

    private Vector3 travelDirection;
    private float spawnTime;

    public void Initialize(Vector3 direction)
    {
        travelDirection = direction.normalized;
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        float moveDistance = speed * Time.deltaTime;

        if (Time.time - spawnTime >= armTime)
        {
            if (Physics.SphereCast(
                transform.position,
                hitRadius,
                travelDirection,
                out RaycastHit hit,
                moveDistance,
                hitLayers,
                QueryTriggerInteraction.Ignore
            ))
            {
                Debug.Log($"Projectile hit: {hit.collider.name}");

                if (impactEffectPrefab != null)
                    Instantiate(impactEffectPrefab, hit.point, Quaternion.identity);

                Destroy(gameObject);
                return;
            }
        }

        transform.position += travelDirection * moveDistance;
    }
}