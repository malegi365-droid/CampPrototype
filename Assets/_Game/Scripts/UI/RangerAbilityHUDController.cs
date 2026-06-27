using UnityEngine;
using UnityEngine.UI;

public class RangerAbilityHUDController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugCooldowns = true;

    [Header("Cooldown Overlays")]
    [SerializeField] private Image overwatchCooldownOverlay;
    [SerializeField] private Image explosiveCooldownOverlay;
    [SerializeField] private Image mobilityCooldownOverlay;
    [SerializeField] private Image overdriveCooldownOverlay;

    [Header("Ability Icons")]
    [SerializeField] private Image overwatchIcon;
    [SerializeField] private Image explosiveIcon;
    [SerializeField] private Image mobilityIcon;
    [SerializeField] private Image overdriveIcon;
    [SerializeField] private Image basicAttackIcon;

    [Header("Cooldown Durations")]
    [SerializeField] private float overwatchCooldown = 14f;
    [SerializeField] private float explosiveCooldown = 5f;
    [SerializeField] private float mobilityCooldown = 2.5f;
    [SerializeField] private float overdriveCooldown = 12f;

    [Header("Cooldown Visual")]
    [SerializeField] private Color cooldownShadeColor = new Color(0f, 0f, 0f, 0.65f);

    [Header("Overdrive Active Visual")]
    [SerializeField] private Color normalIconColor = Color.white;
    [SerializeField] private Color overdriveActiveColor = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private float glowPulseSpeed = 6f;

    private float overwatchReadyTime;
    private float explosiveReadyTime;
    private float mobilityReadyTime;
    private float overdriveReadyTime;

    private bool overdriveActive;

    private void Awake()
    {
        SetupOverlay(overwatchCooldownOverlay);
        SetupOverlay(explosiveCooldownOverlay);
        SetupOverlay(mobilityCooldownOverlay);
        SetupOverlay(overdriveCooldownOverlay);

        if (debugCooldowns)
        {
            Debug.Log(
                $"[RangerAbilityHUDController] Awake on {gameObject.name}. " +
                $"Up:{overwatchCooldownOverlay != null}, " +
                $"Right:{explosiveCooldownOverlay != null}, " +
                $"Left:{mobilityCooldownOverlay != null}, " +
                $"Down:{overdriveCooldownOverlay != null}"
            );
        }
    }

    private void Update()
    {
        UpdateOverlay(overwatchCooldownOverlay, overwatchReadyTime, overwatchCooldown);
        UpdateOverlay(explosiveCooldownOverlay, explosiveReadyTime, explosiveCooldown);
        UpdateOverlay(mobilityCooldownOverlay, mobilityReadyTime, mobilityCooldown);
        UpdateOverlay(overdriveCooldownOverlay, overdriveReadyTime, overdriveCooldown);

        UpdateOverdriveVisuals();
    }

    public void TriggerOverwatchCooldown()
    {
        TriggerPersistentCooldown();
    }

    public void TriggerExplosiveCooldown()
    {
        TriggerSignatureCooldown();
    }

    public void TriggerMobilityCooldown()
    {
        mobilityReadyTime = Time.time + mobilityCooldown;
        LogCooldown("Mobility / Left / Shift", mobilityCooldown);
    }

    public void TriggerOverdriveCooldown()
    {
        overdriveReadyTime = Time.time + overdriveCooldown;
        LogCooldown("Overdrive / Down / R", overdriveCooldown);
    }

    public void TriggerPersistentCooldown()
    {
        overwatchReadyTime = Time.time + overwatchCooldown;
        LogCooldown("Persistent / Up / Q", overwatchCooldown);
    }

    public void TriggerSignatureCooldown()
    {
        explosiveReadyTime = Time.time + explosiveCooldown;
        LogCooldown("Signature / Right / E", explosiveCooldown);
    }

    public void TriggerUltimateCooldown()
    {
        TriggerOverdriveCooldown();
    }

    public void TriggerPiercingCooldown()
    {
        TriggerPersistentCooldown();
    }

    public void TriggerDashCooldown()
    {
        TriggerMobilityCooldown();
    }

    public void TriggerOverchargeCooldown()
    {
        TriggerOverdriveCooldown();
    }

    public void SetOverwatchState(bool active)
    {
        // Intentionally blank for now.
    }

    public void SetOverdriveState(bool active)
    {
        overdriveActive = active;

        if (debugCooldowns)
            Debug.Log($"[RangerAbilityHUDController] {gameObject.name} overdrive active: {active}");
    }

    public void SetOverchargeState(bool active)
    {
        SetOverdriveState(active);
    }

    private void SetupOverlay(Image overlay)
    {
        if (overlay == null)
            return;

        overlay.type = Image.Type.Filled;
        overlay.fillMethod = Image.FillMethod.Vertical;
        overlay.fillOrigin = 1; // Top
        overlay.fillClockwise = false;
        overlay.fillAmount = 0f;

        overlay.raycastTarget = false;

        overlay.color = new Color(
            cooldownShadeColor.r,
            cooldownShadeColor.g,
            cooldownShadeColor.b,
            0f
        );
    }

    private void UpdateOverlay(Image overlay, float readyTime, float cooldownDuration)
    {
        if (overlay == null)
            return;

        float remaining = Mathf.Max(0f, readyTime - Time.time);

        if (remaining <= 0f || cooldownDuration <= 0f)
        {
            overlay.fillAmount = 0f;

            Color clear = cooldownShadeColor;
            clear.a = 0f;
            overlay.color = clear;
            return;
        }

        overlay.fillAmount = remaining / cooldownDuration;
        overlay.color = cooldownShadeColor;
    }

    private void UpdateOverdriveVisuals()
    {
        if (overdriveIcon == null)
            return;

        if (!overdriveActive)
        {
            overdriveIcon.color = normalIconColor;
            return;
        }

        float pulse = (Mathf.Sin(Time.time * glowPulseSpeed) + 1f) * 0.5f;

        overdriveIcon.color = Color.Lerp(
            normalIconColor,
            overdriveActiveColor,
            pulse
        );
    }

    private void LogCooldown(string slotName, float duration)
    {
        if (!debugCooldowns)
            return;

        Debug.Log(
            $"[RangerAbilityHUDController] {gameObject.name} triggered {slotName} cooldown for {duration} seconds."
        );
    }
}