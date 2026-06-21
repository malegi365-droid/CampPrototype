using UnityEngine;

public class ToxicologistPoisonCloudAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode cloudKey = KeyCode.Q;

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

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;

    private float nextCloudTime;

    private void Awake()
    {
        if (aimCamera == null)
            aimCamera = Camera.main;
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

        if (poisonCloudPrefab == null)
        {
            Debug.LogWarning("[ToxicologistPoisonCloudAbility] Missing poison cloud prefab.");
            return;
        }

        if (!TryGetClickWorldPoint(out Vector3 spawnPosition))
        {
            Debug.LogWarning("[ToxicologistPoisonCloudAbility] Could not find valid click location.");
            return;
        }

        GameObject cloudObject = Instantiate(
            poisonCloudPrefab,
            spawnPosition,
            Quaternion.identity
        );

        PoisonCloudZone cloudZone =
            cloudObject.GetComponent<PoisonCloudZone>();

        if (cloudZone != null)
        {
            cloudZone.Initialize(
                cloudDuration,
                tickInterval,
                poisonDuration,
                poisonDamagePerTick,
                enemyLayer
            );
        }

        nextCloudTime = Time.time + cooldown;

        Debug.Log("[ToxicologistPoisonCloudAbility] Poison cloud deployed.");
    }

    private bool TryGetClickWorldPoint(out Vector3 worldPoint)
    {
        worldPoint = transform.position;

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (aimCamera == null)
            return false;

        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            worldPoint = hit.point;
            worldPoint.y = 0f;

            if (limitCastDistance)
            {
                Vector3 fromCaster = worldPoint - transform.position;
                fromCaster.y = 0f;

                if (fromCaster.magnitude > maxCastDistance)
                {
                    fromCaster = fromCaster.normalized * maxCastDistance;
                    worldPoint = transform.position + fromCaster;
                    worldPoint.y = 0f;
                }
            }

            return true;
        }

        return false;
    }
}