using UnityEngine;

public class RangerOverwatchAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode overwatchKey = KeyCode.Q;

    [Header("Overwatch Prefab")]
    [SerializeField] private GameObject overwatchBowPrefab;

    [Header("Settings")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private float cooldown = 14f;
    [SerializeField] private float damageMultiplier = 0.65f;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private float targetRange = 12f;
    [SerializeField] private LayerMask enemyLayer;

    private float nextUseTime;
    private UnitStats rangerStats;

    private void Awake()
    {
        rangerStats = GetComponent<UnitStats>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(overwatchKey))
            TryActivateOverwatch();
    }

    private void TryActivateOverwatch()
    {
        if (Time.time < nextUseTime)
            return;

        if (overwatchBowPrefab == null)
        {
            Debug.LogWarning("[RangerOverwatchAbility] Missing overwatch bow prefab.");
            return;
        }

        GameObject bowObject = Instantiate(
            overwatchBowPrefab,
            transform.position + Vector3.up * 2f,
            Quaternion.identity
        );

        OverwatchBowController bow =
            bowObject.GetComponent<OverwatchBowController>();

        if (bow != null)
        {
            bow.Initialize(
                rangerStats,
                duration,
                damageMultiplier,
                fireInterval,
                targetRange,
                enemyLayer
            );
        }

        nextUseTime = Time.time + cooldown;

        Debug.Log("[RangerOverwatchAbility] Overwatch activated.");
    }
}