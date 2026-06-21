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

    private float nextUseTime;

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
            return;

        PlayerBuffController.Instance.ActivateRapidRegen(
            duration,
            healPerTick,
            tickInterval
        );

        nextUseTime = Time.time + cooldown;

        Debug.Log("[ToxicologistRapidRegenAbility] Rapid Regen activated.");
    }
}