using UnityEngine;

public class ClassUnlockOnDeath : MonoBehaviour
{
    [Header("Unlock Settings")]
    [SerializeField] private PlayerClassType classToUnlock;
    [SerializeField] private bool isBoss = false;

    private HealthController health;
    private bool hasUnlocked = false;

    private void Awake()
    {
        health = GetComponent<HealthController>();
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<HealthController>();

        if (health != null)
            health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDied;
    }

    private void HandleDied(HealthController deadHealth)
    {
        UnlockClass();
    }

    private void UnlockClass()
    {
        if (hasUnlocked)
            return;

        if (!isBoss)
        {
            Debug.Log($"[ClassUnlockOnDeath] {gameObject.name} is not marked as boss. No unlock.");
            return;
        }

        if (ClassUnlockManager.Instance == null)
        {
            Debug.LogWarning("[ClassUnlockOnDeath] No ClassUnlockManager found.");
            return;
        }

        if (ClassUnlockManager.Instance.IsClassUnlocked(classToUnlock))
        {
            Debug.Log($"[ClassUnlockOnDeath] {classToUnlock} is already unlocked.");
            return;
        }

        hasUnlocked = true;

        ClassUnlockManager.Instance.UnlockClass(classToUnlock);

        if (UnlockFeedbackUI.Instance != null)
        {
            UnlockFeedbackUI.Instance.ShowUnlock($"{classToUnlock.ToString().ToUpper()} UNLOCKED");
        }

        Debug.Log($"[ClassUnlockOnDeath] {classToUnlock} unlocked when boss {gameObject.name} died.");
    }
}