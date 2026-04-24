using UnityEngine;

public class PartyMemberControlBridge : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string roleName = "DPS";

    [Header("Player Controlled Components")]
    [Tooltip("These are enabled when this party member is actively controlled by the player.")]
    [SerializeField] private Behaviour[] playerControlledComponents;

    [Header("AI Controlled Components")]
    [Tooltip("These are enabled when this party member is NOT actively controlled by the player.")]
    [SerializeField] private Behaviour[] aiControlledComponents;

    [Header("Optional Visuals")]
    [SerializeField] private GameObject playerControlledIndicator;

    [Header("Optional Camera Anchor")]
    [Tooltip("Optional target transform for camera follow when this member is active.")]
    [SerializeField] private Transform cameraFollowTarget;

    public string RoleName => roleName;
    public bool IsPlayerControlled { get; private set; }
    public Transform CameraFollowTarget => cameraFollowTarget != null ? cameraFollowTarget : transform;

    public void SetPlayerControlled(bool controlled)
    {
        if (IsPlayerControlled == controlled)
            return;

        IsPlayerControlled = controlled;

        SetBehaviours(playerControlledComponents, controlled);
        SetBehaviours(aiControlledComponents, !controlled);

        if (playerControlledIndicator != null)
            playerControlledIndicator.SetActive(controlled);

        OnControlStateChanged(controlled);
    }

    public void ForceRefreshState()
    {
        SetBehaviours(playerControlledComponents, IsPlayerControlled);
        SetBehaviours(aiControlledComponents, !IsPlayerControlled);

        if (playerControlledIndicator != null)
            playerControlledIndicator.SetActive(IsPlayerControlled);

        OnControlStateChanged(IsPlayerControlled);
    }

    private void SetBehaviours(Behaviour[] behaviours, bool enabledState)
    {
        if (behaviours == null)
            return;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
                behaviours[i].enabled = enabledState;
        }
    }

    private void OnControlStateChanged(bool controlled)
    {
        // Optional extension point.
        // If later you need to notify movement/target/camera systems in a more explicit way,
        // this is the safest place to do it without changing the external API.
    }
}