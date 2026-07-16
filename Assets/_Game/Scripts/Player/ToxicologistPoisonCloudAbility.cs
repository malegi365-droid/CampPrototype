using UnityEngine;

public class ToxicologistPoisonCloudAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode cloudKey = KeyCode.E;

    [Header("Cloud Prefab")]
    [SerializeField] private GameObject poisonCloudPrefab;

    [Header("Aiming")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxCastDistance = 20f;
    [SerializeField] private bool limitCastDistance = true;

    [Header("Cloud Settings")]
    [SerializeField] private float cloudDuration = 6f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float poisonDuration = 4f;
    [SerializeField] private float poisonDamagePerTick = 5f;
    [SerializeField] private float cooldown = 10f;

    [Header("Containment Failure Boost")]
    [SerializeField] private float containmentCloudDurationMultiplier = 1.5f;
    [SerializeField] private float containmentPoisonDurationMultiplier = 1.5f;
    [SerializeField] private float containmentDamageMultiplier = 1.5f;
    [SerializeField] private float containmentScaleMultiplier = 1.4f;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    private float nextCloudTime;
    private ToxicologistContainmentFailureAbility containmentFailure;

    public PoisonCloudZone LastSpawnedCloud { get; private set; }

    private void Awake()
    {
        containmentFailure =
            GetComponent<ToxicologistContainmentFailureAbility>();

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (abilityHUD == null)
            abilityHUD = FindHUDByName("ToxicologistAbilityHUD");
    }

    private void Update()
    {
        if (Input.GetKeyDown(cloudKey))
            TrySpawnCloud();
    }

    private void TrySpawnCloud()
    {
        if (Time.time < nextCloudTime)
            return;

        if (!TryGetClickWorldPoint(out Vector3 spawnPosition))
        {
            Debug.LogWarning(
                "[ToxicologistPoisonCloudAbility] Could not find valid click location."
            );

            return;
        }

        SpawnCloud(spawnPosition, true);
    }

    public bool ForceSpawnCloudForShowcase(Vector3 worldPosition)
    {
        worldPosition.y = 0f;

        nextCloudTime = 0f;

        return SpawnCloud(worldPosition, true);
    }

    private bool SpawnCloud(
        Vector3 spawnPosition,
        bool recordAbilityUse
    )
    {
        if (poisonCloudPrefab == null)
        {
            Debug.LogWarning(
                "[ToxicologistPoisonCloudAbility] Missing poison cloud prefab."
            );

            return false;
        }

        float finalCloudDuration = cloudDuration;
        float finalPoisonDuration = poisonDuration;
        float finalPoisonDamagePerTick = poisonDamagePerTick;
        float finalScaleMultiplier = 1f;

        if (IsContainmentFailureActive())
        {
            finalCloudDuration *=
                containmentCloudDurationMultiplier;

            finalPoisonDuration *=
                containmentPoisonDurationMultiplier;

            finalPoisonDamagePerTick *=
                containmentDamageMultiplier;

            finalScaleMultiplier =
                containmentScaleMultiplier;
        }

        GameObject cloudObject = Instantiate(
            poisonCloudPrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (finalScaleMultiplier != 1f)
        {
            cloudObject.transform.localScale *=
                finalScaleMultiplier;
        }

        PoisonCloudZone cloudZone =
            cloudObject.GetComponent<PoisonCloudZone>();

        if (cloudZone == null)
        {
            cloudZone =
                cloudObject.GetComponentInChildren<PoisonCloudZone>();
        }

        LastSpawnedCloud = cloudZone;

        if (cloudZone == null)
        {
            Debug.LogWarning(
                "[ToxicologistPoisonCloudAbility] " +
                "Spawned cloud is missing PoisonCloudZone."
            );
        }
        else
        {
            cloudZone.Initialize(
                finalCloudDuration,
                tickInterval,
                finalPoisonDuration,
                finalPoisonDamagePerTick,
                enemyLayer
            );
        }

        if (abilityHUD != null)
        {
            abilityHUD.TriggerSignatureCooldown();
        }
        else
        {
            Debug.LogWarning(
                "[ToxicologistPoisonCloudAbility] " +
                "Missing Ability HUD reference."
            );
        }

        nextCloudTime = Time.time + cooldown;

        if (recordAbilityUse)
        {
            AbilityWeaveManager.Instance?.RecordAbilityUsed(
                CombatClassType.Toxicologist,
                AbilitySlotType.Signature
            );
        }

        Debug.Log(
            "[ToxicologistPoisonCloudAbility] Poison cloud deployed. " +
            $"Duration={finalCloudDuration}, " +
            $"PoisonDuration={finalPoisonDuration}, " +
            $"Damage={finalPoisonDamagePerTick}, " +
            $"Scale={finalScaleMultiplier}, " +
            $"CloudFound={LastSpawnedCloud != null}"
        );

        return LastSpawnedCloud != null;
    }

    private bool TryGetClickWorldPoint(
        out Vector3 worldPoint
    )
    {
        worldPoint = transform.position;

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (aimCamera == null)
            return false;

        Ray ray =
            aimCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            100f,
            groundLayer
        ))
        {
            worldPoint = hit.point;
            worldPoint.y = 0f;

            if (limitCastDistance)
            {
                Vector3 fromCaster =
                    worldPoint - transform.position;

                fromCaster.y = 0f;

                if (fromCaster.magnitude >
                    maxCastDistance)
                {
                    fromCaster =
                        fromCaster.normalized *
                        maxCastDistance;

                    worldPoint =
                        transform.position +
                        fromCaster;

                    worldPoint.y = 0f;
                }
            }

            return true;
        }

        return false;
    }

    private bool IsContainmentFailureActive()
    {
        return containmentFailure != null &&
               containmentFailure
                   .IsContainmentFailureActive;
    }

    private RangerAbilityHUDController FindHUDByName(
        string hudName
    )
    {
        RangerAbilityHUDController[] huds =
            FindObjectsByType<RangerAbilityHUDController>(
                FindObjectsInactive.Include
            );

        foreach (
            RangerAbilityHUDController hud in huds
        )
        {
            if (hud.gameObject.name == hudName)
                return hud;
        }

        return null;
    }
}