using UnityEngine;
using UnityEngine.UI;

public class RangerAbilityHUDController : MonoBehaviour
{
    [Header("Cooldown Overlays")]
    [SerializeField] private Image piercingCooldownOverlay;
    [SerializeField] private Image explosiveCooldownOverlay;
    [SerializeField] private Image dashCooldownOverlay;
    [SerializeField] private Image overchargeCooldownOverlay;

    [Header("Ability Icons")]
    [SerializeField] private Image piercingIcon;
    [SerializeField] private Image explosiveIcon;
    [SerializeField] private Image dashIcon;
    [SerializeField] private Image overchargeIcon;

    [Header("Cooldown Durations")]
    [SerializeField] private float piercingCooldown = 3f;
    [SerializeField] private float explosiveCooldown = 5f;
    [SerializeField] private float dashCooldown = 2.5f;
    [SerializeField] private float overchargeCooldown = 12f;

    [Header("Overcharge Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField]
    private Color overchargeActiveColor =
        new Color(1f, 0.55f, 0.1f);

    [SerializeField] private float glowPulseSpeed = 6f;

    private float piercingReadyTime;
    private float explosiveReadyTime;
    private float dashReadyTime;
    private float overchargeReadyTime;

    private bool overchargeActive;

    private void Update()
    {
        UpdateOverlay(
            piercingCooldownOverlay,
            piercingReadyTime,
            piercingCooldown
        );

        UpdateOverlay(
            explosiveCooldownOverlay,
            explosiveReadyTime,
            explosiveCooldown
        );

        UpdateOverlay(
            dashCooldownOverlay,
            dashReadyTime,
            dashCooldown
        );

        UpdateOverlay(
            overchargeCooldownOverlay,
            overchargeReadyTime,
            overchargeCooldown
        );

        UpdateOverchargeVisuals();
    }

    public void TriggerPiercingCooldown()
    {
        piercingReadyTime = Time.time + piercingCooldown;
    }

    public void TriggerExplosiveCooldown()
    {
        explosiveReadyTime = Time.time + explosiveCooldown;
    }

    public void TriggerDashCooldown()
    {
        dashReadyTime = Time.time + dashCooldown;
    }

    public void TriggerOverchargeCooldown()
    {
        overchargeReadyTime = Time.time + overchargeCooldown;
    }

    public void SetOverchargeState(bool active)
    {
        overchargeActive = active;
    }

    private void UpdateOverlay(
        Image overlay,
        float readyTime,
        float cooldownDuration
    )
    {
        if (overlay == null)
            return;

        float remaining =
            Mathf.Max(0f, readyTime - Time.time);

        overlay.fillAmount =
            remaining / cooldownDuration;

        Color c = overlay.color;

        if (remaining <= 0f)
            c.a = 0f;
        else
            c.a = 0.55f;

        overlay.color = c;
    }

    private void UpdateOverchargeVisuals()
    {
        if (overchargeIcon == null)
            return;

        if (!overchargeActive)
        {
            overchargeIcon.color = normalColor;
            return;
        }

        float pulse =
            (Mathf.Sin(Time.time * glowPulseSpeed) + 1f) * 0.5f;

        overchargeIcon.color =
            Color.Lerp(
                normalColor,
                overchargeActiveColor,
                pulse
            );
    }
}