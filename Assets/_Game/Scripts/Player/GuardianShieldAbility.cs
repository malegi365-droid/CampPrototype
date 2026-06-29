using UnityEngine;

public class GuardianShieldAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode shieldKey = KeyCode.Q;

    [Header("Shield Settings")]
    [SerializeField] private float shieldDuration = 6f;
    [SerializeField] private float damageReduction = 0.5f;
    [SerializeField] private float cooldown = 12f;

    [Header("Visuals")]
    [SerializeField] private GameObject shieldVFXPrefab;
    [SerializeField] private Transform shieldVFXAnchor;

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    private float nextShieldTime;
    private GameObject activeShieldVFX;

    private void Awake()
    {
        if (abilityHUD == null)
            abilityHUD = FindHUDByName("GuardianAbilityHUD");
    }

    private void Update()
    {
        if (Input.GetKeyDown(shieldKey))
            TryActivateShield();
    }

    private void TryActivateShield()
    {
        if (Time.time < nextShieldTime)
            return;

        if (PlayerBuffController.Instance == null)
        {
            Debug.LogWarning("[GuardianShieldAbility] No PlayerBuffController found.");
            return;
        }

        PlayerBuffController.Instance.ActivateGuardianShield(
            shieldDuration,
            damageReduction
        );

        SpawnShieldVFX();

        if (abilityHUD != null)
            abilityHUD.TriggerPersistentCooldown();
        else
            Debug.LogWarning("[GuardianShieldAbility] Missing Ability HUD reference.");

        nextShieldTime = Time.time + cooldown;

        Debug.Log("[GuardianShieldAbility] Guardian Shield activated.");
    }

    private void SpawnShieldVFX()
    {
        if (shieldVFXPrefab == null)
            return;

        Transform anchor = shieldVFXAnchor != null ? shieldVFXAnchor : transform;

        if (activeShieldVFX != null)
            Destroy(activeShieldVFX);

        activeShieldVFX = Instantiate(
            shieldVFXPrefab,
            anchor.position,
            anchor.rotation,
            anchor
        );

        Destroy(activeShieldVFX, shieldDuration);
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