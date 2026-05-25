using UnityEngine;
using UnityEngine.UI;

public class DPSAbilityHUDController : MonoBehaviour
{
    [Header("Cooldown Overlays")]
    [SerializeField] private Image piercingCooldownOverlay;
    [SerializeField] private Image explosiveCooldownOverlay;

    [Header("Cooldown Durations")]
    [SerializeField] private float piercingCooldown = 3f;
    [SerializeField] private float explosiveCooldown = 5f;

    private float piercingReadyTime;
    private float explosiveReadyTime;

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
    }

    public void TriggerPiercingCooldown()
    {
        piercingReadyTime = Time.time + piercingCooldown;
    }

    public void TriggerExplosiveCooldown()
    {
        explosiveReadyTime = Time.time + explosiveCooldown;
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
}