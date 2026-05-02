using UnityEngine;

public class ClassUnlockTrigger : MonoBehaviour
{
    [SerializeField] private PlayerClassType classToUnlock;
    [SerializeField] private bool destroyOnUse = true;

    private void OnTriggerEnter(Collider other)
    {
        PartyMemberControlBridge player = other.GetComponent<PartyMemberControlBridge>();

        if (player == null || !player.IsPlayerControlled)
            return;

        if (ClassUnlockManager.Instance == null)
        {
            Debug.LogWarning("[ClassUnlockTrigger] No ClassUnlockManager found.");
            return;
        }

        if (!ClassUnlockManager.Instance.IsClassUnlocked(classToUnlock))
        {
            ClassUnlockManager.Instance.UnlockClass(classToUnlock);

            if (UnlockFeedbackUI.Instance != null)
            {
                UnlockFeedbackUI.Instance.ShowUnlock($"{classToUnlock.ToString().ToUpper()} UNLOCKED");
            }

            Debug.Log($"[ClassUnlockTrigger] Unlocked {classToUnlock}");
        }

        if (destroyOnUse)
        {
            Destroy(gameObject);
        }
    }
}