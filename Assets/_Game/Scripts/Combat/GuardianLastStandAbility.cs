using UnityEngine;

public class GuardianLastStandAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode ultimateKey = KeyCode.R;

    [Header("Last Stand")]
    [SerializeField] private float duration = 8f;
    [SerializeField] private float cooldown = 20f;
    [SerializeField] private float damageReduction = 0.75f;

    [Header("Activation Shockwave")]
    [SerializeField] private float shockwaveRadius = 5f;
    [SerializeField] private float shockwaveDamage = 35f;
    [SerializeField] private float shockwavePushDistance = 3f;
    [SerializeField] private float shockwaveStaggerDuration = 0.45f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    public bool IsLastStandActive { get; private set; }

    private UnitStats ownerStats;
    private float nextUseTime;
    private float endTime;

    private void Awake()
    {
        ownerStats = GetComponent<UnitStats>();

        if (abilityHUD == null)
            abilityHUD = FindHUDByName("GuardianAbilityHUD");
    }

    private void Update()
    {
        if (Input.GetKeyDown(ultimateKey))
            TryActivate();

        if (IsLastStandActive && Time.time >= endTime)
            EndLastStand();
    }

    private void TryActivate()
    {
        if (Time.time < nextUseTime)
            return;

        ActivateLastStand();
    }

    private void ActivateLastStand()
    {
        IsLastStandActive = true;
        endTime = Time.time + duration;
        nextUseTime = Time.time + cooldown;

        if (PlayerBuffController.Instance != null)
            PlayerBuffController.Instance.SetGuardianLastStandActive(true, damageReduction);
        else
            Debug.LogWarning("[GuardianLastStandAbility] Missing PlayerBuffController instance.");

        TriggerShockwave();

        if (abilityHUD != null)
        {
            abilityHUD.TriggerOverdriveCooldown();
            abilityHUD.SetOverdriveState(true);
        }
        else
        {
            Debug.LogWarning("[GuardianLastStandAbility] Missing Ability HUD reference.");
        }

        AbilityWeaveManager.Instance?.RecordAbilityUsed(
            CombatClassType.Guardian,
            AbilitySlotType.Ultimate
        );

        Debug.Log("[GuardianLastStandAbility] Last Stand activated.");
    }

    private void TriggerShockwave()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            shockwaveRadius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            UnitStats targetStats = hit.GetComponentInParent<UnitStats>();

            if (targetStats == null || targetStats.role != UnitRole.Enemy)
                continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(shockwaveDamage, ownerStats);

            PushEnemyAway(targetStats.transform);

            EnemyHitReactionController reaction =
                targetStats.GetComponent<EnemyHitReactionController>();

            if (reaction != null)
                reaction.ApplyStagger(shockwaveStaggerDuration);

            Debug.Log($"[GuardianLastStandAbility] Shockwave hit {targetStats.name} for {shockwaveDamage}.");
        }
    }

    private void PushEnemyAway(Transform enemyTransform)
    {
        Vector3 direction = enemyTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            direction = transform.forward;

        direction.Normalize();

        Vector3 push = direction * shockwavePushDistance;
        push.y = 0f;

        CharacterController enemyController =
            enemyTransform.GetComponent<CharacterController>();

        if (enemyController != null)
        {
            enemyController.Move(push);
            return;
        }

        enemyTransform.position += push;
    }

    private void EndLastStand()
    {
        IsLastStandActive = false;

        if (PlayerBuffController.Instance != null)
            PlayerBuffController.Instance.SetGuardianLastStandActive(false);

        if (abilityHUD != null)
            abilityHUD.SetOverdriveState(false);

        Debug.Log("[GuardianLastStandAbility] Last Stand ended.");
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }
}