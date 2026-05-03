using UnityEngine;
using UnityEngine.SceneManagement;

public class BreachToControlRoomTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string controlRoomSceneName = "ControlRoom";

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnlyOnce)
            return;

        PartyMemberControlBridge player = other.GetComponent<PartyMemberControlBridge>();
        if (player == null)
            player = other.GetComponentInParent<PartyMemberControlBridge>();

        if (player == null || !player.IsPlayerControlled)
            return;

        hasTriggered = true;

        Debug.Log("[BreachToControlRoomTrigger] Breach triggered. Loading Control Room.");

        SceneManager.LoadScene(controlRoomSceneName);
    }
}