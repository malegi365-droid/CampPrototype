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

    private float nextShieldTime;
    private GameObject activeShieldVFX;

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
}