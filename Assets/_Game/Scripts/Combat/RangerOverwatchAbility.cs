using UnityEngine;

public class RangerOverwatchAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode overwatchKey = KeyCode.Q;

    [Header("Overwatch Prefab")]
    [SerializeField] private GameObject overwatchDronePrefab;

    [Header("Settings")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private float cooldown = 14f;
    [SerializeField] private float damageMultiplier = 0.65f;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private float targetRange = 12f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    private float nextUseTime;
    private UnitStats rangerStats;

    private void Awake()
    {
        rangerStats = GetComponent<UnitStats>();

        if (abilityHUD == null)
            abilityHUD = FindHUDByName("RangerAbilityHUD");
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

        if (overwatchDronePrefab == null)
        {
            Debug.LogWarning("[RangerOverwatchAbility] Missing overwatch drone prefab.");
            return;
        }

        GameObject droneObject = Instantiate(
            overwatchDronePrefab,
            transform.position + Vector3.up * 2f,
            Quaternion.identity
        );

        OverwatchDroneController drone =
            droneObject.GetComponent<OverwatchDroneController>();

        if (drone == null)
            drone = droneObject.GetComponentInChildren<OverwatchDroneController>();

        if (drone != null)
        {
            drone.Initialize(
                rangerStats,
                duration,
                damageMultiplier,
                fireInterval,
                targetRange,
                enemyLayer
            );
        }
        else
        {
            Debug.LogWarning("[RangerOverwatchAbility] Spawned prefab is missing OverwatchDroneController.");
        }

        nextUseTime = Time.time + cooldown;

        if (abilityHUD != null)
        {
            abilityHUD.TriggerPersistentCooldown();
            abilityHUD.SetOverwatchState(true);
        }
        else
        {
            Debug.LogWarning("[RangerOverwatchAbility] Missing Ability HUD reference.");
        }

        AbilityWeaveManager.Instance?.RecordAbilityUsed(
            CombatClassType.Ranger,
            AbilitySlotType.Persistent
        );

        Invoke(nameof(EndOverwatchHUDState), duration);

        Debug.Log("[RangerOverwatchAbility] Overwatch drone activated.");
    }

    private void EndOverwatchHUDState()
    {
        if (abilityHUD != null)
            abilityHUD.SetOverwatchState(false);
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