using UnityEngine;

public class ToxicologistRapidRegenAbility : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode regenKey = KeyCode.Q;

    [Header("Rapid Regen")]
    [SerializeField] private float duration = 8f;
    [SerializeField] private float healPerTick = 5f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float cooldown = 12f;

    [Header("Containment Failure Boost")]
    [SerializeField] private float containmentHealMultiplier = 2f;
    [SerializeField] private float containmentTickIntervalMultiplier = 0.5f;

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    private float nextUseTime;
    private ToxicologistContainmentFailureAbility containmentFailure;

    private void Awake()
    {
        containmentFailure = GetComponent<ToxicologistContainmentFailureAbility>();

        if (abilityHUD == null)
            abilityHUD = GetComponentInChildren<RangerAbilityHUDController>();

        if (abilityHUD == null)
            abilityHUD = FindHUDByName("ToxicologistAbilityHUD");
    }

    private void Update()
    {
        if (Input.GetKeyDown(regenKey))
            TryActivate();
    }

    private void TryActivate()
    {
        if (Time.time < nextUseTime)
            return;

        if (PlayerBuffController.Instance == null)
        {
            Debug.LogWarning("[ToxicologistRapidRegenAbility] Missing PlayerBuffController instance.");
            return;
        }

        float finalHealPerTick = healPerTick;
        float finalTickInterval = tickInterval;

        if (IsContainmentFailureActive())
        {
            finalHealPerTick *= containmentHealMultiplier;
            finalTickInterval *= containmentTickIntervalMultiplier;
        }

        PlayerBuffController.Instance.ActivateRapidRegen(
            duration,
            finalHealPerTick,
            finalTickInterval
        );

        if (abilityHUD != null)
            abilityHUD.TriggerPersistentCooldown();
        else
            Debug.LogWarning("[ToxicologistRapidRegenAbility] Missing Ability HUD reference.");

        nextUseTime = Time.time + cooldown;

        Debug.Log(
            $"[ToxicologistRapidRegenAbility] Rapid Regen activated. Heal={finalHealPerTick}, TickInterval={finalTickInterval}"
        );
    }

    private bool IsContainmentFailureActive()
    {
        return containmentFailure != null &&
               containmentFailure.IsContainmentFailureActive;
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