using UnityEngine;

[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(UnitStats))]
public class EnemyWorldHPBarSpawner : MonoBehaviour
{
    [Header("HP Bar Prefab")]
    [SerializeField] private EnemyWorldHPBarController hpBarPrefab;

    private HealthController health;
    private UnitStats stats;
    private EnemyWorldHPBarController spawnedBar;

    private void Start()
    {
        health = GetComponent<HealthController>();
        stats = GetComponent<UnitStats>();

        if (stats.role != UnitRole.Enemy)
            return;

        if (hpBarPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing HP bar prefab.");
            return;
        }

        spawnedBar = Instantiate(hpBarPrefab);
        spawnedBar.Initialize(health, stats);
    }

    private void OnDestroy()
    {
        if (spawnedBar != null)
            Destroy(spawnedBar.gameObject);
    }
}