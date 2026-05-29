using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class EnemyDropOnDeath : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private GameObject pickupPrefab;
    [SerializeField] private int dropAmount = 1;
    [SerializeField] private float dropChance = 1f;

    [Header("Spawn Position")]
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private float scatterRadius = 0.35f;

    private HealthController healthController;

    private void Awake()
    {
        healthController = GetComponent<HealthController>();
    }

    private void OnEnable()
    {
        if (healthController != null)
            healthController.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        if (healthController != null)
            healthController.OnDied -= HandleDeath;
    }

    private void HandleDeath(HealthController deadUnit)
    {
        if (pickupPrefab == null)
            return;

        if (Random.value > dropChance)
            return;

        for (int i = 0; i < dropAmount; i++)
        {
            Vector2 scatter = Random.insideUnitCircle * scatterRadius;

            Vector3 spawnPosition =
                transform.position +
                dropOffset +
                new Vector3(scatter.x, 0f, scatter.y);

            Instantiate(
                pickupPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }
    }
}