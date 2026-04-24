using UnityEngine;

public class PartyControlManager : MonoBehaviour
{
    [Header("Party Members")]
    [SerializeField] private PartyMemberControlBridge tank;
    [SerializeField] private PartyMemberControlBridge dps;
    [SerializeField] private PartyMemberControlBridge healer;

    [Header("Starting Control")]
    [SerializeField] private PartyMemberControlBridge startingMember;

    [Header("Keybinds")]
    [SerializeField] private KeyCode tankKey = KeyCode.F1;
    [SerializeField] private KeyCode dpsKey = KeyCode.F2;
    [SerializeField] private KeyCode healerKey = KeyCode.F3;

    [Header("Optional Camera")]
    [SerializeField] private Transform cameraFollowProxy;

    public PartyMemberControlBridge CurrentMember { get; private set; }

    private void Start()
    {
        if (startingMember == null)
            startingMember = dps;

        ForceSwitchControl(startingMember);
    }

    private void Update()
    {
        if (Input.GetKeyDown(tankKey))
            ForceSwitchControl(tank);

        if (Input.GetKeyDown(dpsKey))
            ForceSwitchControl(dps);

        if (Input.GetKeyDown(healerKey))
            ForceSwitchControl(healer);
    }

    public void ForceSwitchControl(PartyMemberControlBridge newMember)
    {
        if (newMember == null)
        {
            Debug.LogWarning("[PartyControlManager] Tried to switch to a null party member.");
            return;
        }

        SetMemberState(tank, tank == newMember);
        SetMemberState(dps, dps == newMember);
        SetMemberState(healer, healer == newMember);

        CurrentMember = newMember;

        UpdateCameraProxy();

        Debug.Log($"[PartyControlManager] Player now controlling: {CurrentMember.RoleName}");
    }

    private void SetMemberState(PartyMemberControlBridge member, bool shouldBePlayerControlled)
    {
        if (member == null)
            return;

        member.SetPlayerControlled(shouldBePlayerControlled);
        member.ForceRefreshState();
    }

    private void UpdateCameraProxy()
    {
        if (cameraFollowProxy == null || CurrentMember == null)
            return;

        cameraFollowProxy.position = CurrentMember.CameraFollowTarget.position;
        cameraFollowProxy.SetParent(CurrentMember.CameraFollowTarget, true);
    }
}