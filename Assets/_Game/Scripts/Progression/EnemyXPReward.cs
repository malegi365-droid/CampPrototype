using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class EnemyXPReward : MonoBehaviour
{
    public int xpReward = 20;
    public XPController playerXP;

    private HealthController healthController;

    private void Awake()
    {
        healthController = GetComponent<HealthController>();
        healthController.OnDied += HandleDeath;
    }

    private void OnDestroy()
    {
        if (healthController != null)
        {
            healthController.OnDied -= HandleDeath;
        }
    }

    private void HandleDeath(HealthController deadUnit)
    {
        if (playerXP != null)
        {
            playerXP.GainXP(xpReward);
        }
    }
}