using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class EnemyXPReward : MonoBehaviour
{
    [SerializeField] private int xpReward = 20;

    private HealthController healthController;
    private PartyProgressionController partyProgression;

    private void Awake()
    {
        healthController = GetComponent<HealthController>();
        partyProgression = FindFirstObjectByType<PartyProgressionController>();
    }

    private void OnEnable()
    {
        if (healthController != null)
        {
            healthController.OnDied += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (healthController != null)
        {
            healthController.OnDied -= HandleDeath;
        }
    }

    private void HandleDeath(HealthController deadUnit)
    {
        if (partyProgression != null)
        {
            partyProgression.GainXP(xpReward);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No PartyProgressionController found for XP reward.");
        }
    }
}