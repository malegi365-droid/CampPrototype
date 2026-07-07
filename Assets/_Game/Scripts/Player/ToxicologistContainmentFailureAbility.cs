using UnityEngine;

public class ToxicologistContainmentFailureAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode ultimateKey = KeyCode.R;

    [Header("Containment Failure")]
    [SerializeField] private float duration = 8f;
    [SerializeField] private float cooldown = 20f;
    [SerializeField] private float auraRadius = 5f;
    [SerializeField] private float damagePerTick = 6f;
    [SerializeField] private float tickInterval = 0.35f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("VFX")]
    [SerializeField] private GameObject activeVFXPrefab;
    [SerializeField] private Vector3 vfxOffset = new Vector3(0f, 1f, 0f);

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    public bool IsContainmentFailureActive { get; private set; }

    private float nextUseTime;
    private float endTime;
    private float nextTickTime;
    private GameObject activeVFX;

    private void Awake()
    {
        if (abilityHUD == null)
            abilityHUD = FindHUDByName("ToxicologistAbilityHUD");
    }

    private void Update()
    {
        if (Input.GetKeyDown(ultimateKey))
            TryActivate();

        if (IsContainmentFailureActive)
            UpdateContainmentFailure();
    }

    private void TryActivate()
    {
        if (Time.time < nextUseTime)
            return;

        ActivateContainmentFailure();
    }

    private void ActivateContainmentFailure()
    {
        IsContainmentFailureActive = true;

        endTime = Time.time + duration;
        nextUseTime = Time.time + cooldown;
        nextTickTime = Time.time;

        SpawnVFX();

        if (abilityHUD != null)
        {
            abilityHUD.TriggerOverdriveCooldown();
            abilityHUD.SetOverdriveState(true);
        }
        else
        {
            Debug.LogWarning("[ToxicologistContainmentFailureAbility] Missing Ability HUD reference.");
        }

        AbilityWeaveManager.Instance?.RecordAbilityUsed(
            CombatClassType.Toxicologist,
            AbilitySlotType.Ultimate
        );

        Debug.Log("[ToxicologistContainmentFailureAbility] Containment Failure activated.");
    }

    private void UpdateContainmentFailure()
    {
        FollowVFX();

        if (Time.time >= endTime)
        {
            EndContainmentFailure();
            return;
        }

        if (Time.time >= nextTickTime)
        {
            DamageEnemiesInAura();
            nextTickTime = Time.time + tickInterval;
        }
    }

    private void DamageEnemiesInAura()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            auraRadius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            UnitStats stats = hit.GetComponentInParent<UnitStats>();

            if (stats == null || stats.role != UnitRole.Enemy)
                continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            damageable.TakeDamage(damagePerTick, GetComponent<UnitStats>());
        }
    }

    private void SpawnVFX()
    {
        if (activeVFXPrefab == null)
            return;

        if (activeVFX != null)
            Destroy(activeVFX);

        activeVFX = Instantiate(
            activeVFXPrefab,
            transform.position + vfxOffset,
            Quaternion.identity
        );
    }

    private void FollowVFX()
    {
        if (activeVFX == null)
            return;

        activeVFX.transform.position = transform.position + vfxOffset;
    }

    private void EndContainmentFailure()
    {
        IsContainmentFailureActive = false;

        if (abilityHUD != null)
            abilityHUD.SetOverdriveState(false);

        if (activeVFX != null)
            Destroy(activeVFX);

        Debug.Log("[ToxicologistContainmentFailureAbility] Containment Failure ended.");
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}