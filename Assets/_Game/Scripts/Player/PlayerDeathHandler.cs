using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(HealthController))]
public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private float resetDelay = 2f;

    private HealthController healthController;
    private bool handlingDeath = false;

    private void Awake()
    {
        healthController = GetComponent<HealthController>();
    }

    private void Start()
    {
        if (healthController != null)
        {
            healthController.OnDied += HandleDeath;
        }
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
        if (handlingDeath) return;
        handlingDeath = true;

        Debug.Log("Player died. Encounter failed. Reloading scene...");
        Invoke(nameof(ReloadScene), resetDelay);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}