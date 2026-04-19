using UnityEngine;

[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(UnitStats))]
public class EnemyRespawnController : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 8f;

    private HealthController healthController;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private bool isRespawning = false;

    private void Awake()
    {
        healthController = GetComponent<HealthController>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    private void OnEnable()
    {
        if (healthController != null)
        {
            healthController.OnDied += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (healthController != null)
        {
            healthController.OnDied -= HandleDeath;
        }
    }

    private void HandleDeath(HealthController deadUnit)
    {
        if (isRespawning) return;
        isRespawning = true;

        Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        healthController.ResetHealth();

        AutoAttackController autoAttack = GetComponent<AutoAttackController>();
        if (autoAttack != null)
        {
            autoAttack.SetTarget(null);
        }

        ThreatTable threatTable = GetComponent<ThreatTable>();
        if (threatTable != null)
        {
            threatTable.ClearAllThreat();
        }

        isRespawning = false;

        Debug.Log($"{gameObject.name} respawned.");
    }
}