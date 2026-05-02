using UnityEngine;

public class PartyControlManager : MonoBehaviour
{
    [Header("Class Bodies")]
    [SerializeField] private PartyMemberControlBridge tank;
    [SerializeField] private PartyMemberControlBridge dps;
    [SerializeField] private PartyMemberControlBridge healer;

    [Header("Starting Class")]
    [SerializeField] private PartyMemberControlBridge startingMember;

    [Header("Keybinds")]
    [SerializeField] private KeyCode tankKey = KeyCode.F1;
    [SerializeField] private KeyCode dpsKey = KeyCode.F2;
    [SerializeField] private KeyCode healerKey = KeyCode.F3;

    [Header("Camera")]
    [SerializeField] private CameraFollowProxy cameraFollowProxy;

    [Header("Switch Settings")]
    [SerializeField] private bool transferTargetOnSwitch = true;

    public PartyMemberControlBridge CurrentMember { get; private set; }

    private void Start()
    {
        if (startingMember == null)
            startingMember = dps;

        ActivateOnly(startingMember, true, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(tankKey))
            ActivateOnly(tank, false);

        if (Input.GetKeyDown(dpsKey))
            ActivateOnly(dps, false);

        if (Input.GetKeyDown(healerKey))
            ActivateOnly(healer, false);
    }

    public void ActivateOnly(PartyMemberControlBridge newMember, bool snapCamera)
    {
        ActivateOnly(newMember, snapCamera, false);
    }

    private void ActivateOnly(PartyMemberControlBridge newMember, bool snapCamera, bool bypassUnlockCheck)
    {
        if (newMember == null)
        {
            Debug.LogWarning("[PartyControlManager] Tried to activate null member.");
            return;
        }

        PlayerClassType requestedClass = GetClassTypeForMember(newMember);

        if (!bypassUnlockCheck)
        {
            if (ClassUnlockManager.Instance == null)
            {
                Debug.LogWarning("[PartyControlManager] No ClassUnlockManager found in scene.");
                return;
            }

            if (!ClassUnlockManager.Instance.IsClassUnlocked(requestedClass))
            {
                Debug.Log($"[PartyControlManager] {requestedClass} is locked.");
                return;
            }
        }

        if (newMember == CurrentMember)
            return;

        Vector3 switchPosition = newMember.transform.position;
        Quaternion switchRotation = newMember.transform.rotation;
        Transform previousTarget = null;

        if (CurrentMember != null)
        {
            switchPosition = CurrentMember.transform.position;
            switchRotation = CurrentMember.transform.rotation;

            TargetingController oldTargeting = CurrentMember.GetComponent<TargetingController>();
            if (oldTargeting != null)
                previousTarget = oldTargeting.GetCurrentTarget();
        }

        DeactivateMember(tank);
        DeactivateMember(dps);
        DeactivateMember(healer);

        newMember.transform.position = switchPosition;
        newMember.transform.rotation = switchRotation;

        newMember.gameObject.SetActive(true);
        newMember.SetPlayerControlled(true);
        newMember.ForceRefreshState();

        if (transferTargetOnSwitch && previousTarget != null)
        {
            TargetingController newTargeting = newMember.GetComponent<TargetingController>();
            if (newTargeting != null)
                newTargeting.SetTarget(previousTarget);
        }

        CurrentMember = newMember;

        if (cameraFollowProxy != null)
            cameraFollowProxy.SetTarget(CurrentMember.CameraFollowTarget, snapCamera);

        Debug.Log($"[PartyControlManager] Active class: {CurrentMember.RoleName}");
    }

    private PlayerClassType GetClassTypeForMember(PartyMemberControlBridge member)
    {
        if (member == tank)
            return PlayerClassType.Tank;

        if (member == healer)
            return PlayerClassType.Healer;

        return PlayerClassType.DPS;
    }

    private void DeactivateMember(PartyMemberControlBridge member)
    {
        if (member == null)
            return;

        member.SetPlayerControlled(false);
        member.ForceRefreshState();

        TargetingController targeting = member.GetComponent<TargetingController>();
        if (targeting != null)
            targeting.ClearTarget();

        AutoAttackController autoAttack = member.GetComponent<AutoAttackController>();
        if (autoAttack != null)
            autoAttack.SetTarget(null);

        member.gameObject.SetActive(false);
    }
}