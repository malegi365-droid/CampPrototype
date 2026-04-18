using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(UnitStats))]
public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private float resetDelay = 2f;

    private HealthController healthController;
    private UnitStats stats;
    private bool handlingDeath = false;

    private void Awake()
    {
        healthController = GetComponent<HealthController>();
        stats = GetComponent<UnitStats>();
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
        if (handlingDeath) return;
        handlingDeath = true;

        Debug.Log("Player died. Encounter failed.");

        Invoke(nameof(ReloadScene), resetDelay);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}