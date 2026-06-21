using UnityEngine;

public class PartyControlManager : MonoBehaviour
{
    [Header("Class Bodies")]
    [SerializeField] private PartyMemberControlBridge guardian;
    [SerializeField] private PartyMemberControlBridge ranger;
    [SerializeField] private PartyMemberControlBridge toxicologist;

    [Header("Starting Class")]
    [SerializeField] private PartyMemberControlBridge startingMember;

    [Header("Keybinds")]
    [SerializeField] private KeyCode guardianKey = KeyCode.F1;
    [SerializeField] private KeyCode rangerKey = KeyCode.F2;
    [SerializeField] private KeyCode toxicologistKey = KeyCode.F3;

    [Header("Camera")]
    [SerializeField] private CameraFollowProxy cameraFollowProxy;

    [Header("Switch Settings")]
    [SerializeField] private bool transferTargetOnSwitch = true;

    [Header("Switch VFX")]
    [SerializeField] private GameObject classSwitchVFXPrefab;
    [SerializeField] private float switchVFXLifetime = 1.5f;
    [SerializeField] private bool spawnVFXBeforeSwitch = true;
    [SerializeField] private bool spawnVFXAfterSwitch = true;

    [Header("Switch Screen FX")]
    [SerializeField] private ClassSwitchScreenFX classSwitchScreenFX;

    public PartyMemberControlBridge CurrentMember { get; private set; }

    private void Start()
    {
        if (startingMember == null)
            startingMember = ranger;

        ActivateOnly(startingMember, true, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(guardianKey))
            ActivateOnly(guardian, false);

        if (Input.GetKeyDown(rangerKey))
            ActivateOnly(ranger, false);

        if (Input.GetKeyDown(toxicologistKey))
            ActivateOnly(toxicologist, false);
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

            if (spawnVFXBeforeSwitch)
                SpawnClassSwitchVFX(switchPosition);

            TargetingController oldTargeting = CurrentMember.GetComponent<TargetingController>();
            if (oldTargeting != null)
                previousTarget = oldTargeting.GetCurrentTarget();
        }

        DeactivateMember(guardian);
        DeactivateMember(ranger);
        DeactivateMember(toxicologist);

        newMember.transform.position = switchPosition;
        newMember.transform.rotation = switchRotation;

        newMember.gameObject.SetActive(true);
        newMember.SetPlayerControlled(true);
        newMember.ForceRefreshState();

        if (spawnVFXAfterSwitch)
            SpawnClassSwitchVFX(switchPosition);

        if (transferTargetOnSwitch && previousTarget != null)
        {
            TargetingController newTargeting = newMember.GetComponent<TargetingController>();
            if (newTargeting != null)
                newTargeting.SetTarget(previousTarget);
        }

        CurrentMember = newMember;

        UpdateCameraTarget(snapCamera);

        if (classSwitchScreenFX != null)
            classSwitchScreenFX.PlaySwitchPulse();

        Debug.Log($"[PartyControlManager] Active class: {CurrentMember.RoleName}");
    }

    private void UpdateCameraTarget(bool snapCamera)
    {
        if (cameraFollowProxy == null || CurrentMember == null)
            return;

        Transform followTarget = CurrentMember.CameraFollowTarget;

        if (followTarget == null)
        {
            followTarget = CurrentMember.transform;
            Debug.LogWarning($"[PartyControlManager] {CurrentMember.RoleName} has no CameraFollowTarget. Falling back to root transform.");
        }

        cameraFollowProxy.SetTarget(followTarget, snapCamera);
    }

    private void SpawnClassSwitchVFX(Vector3 position)
    {
        if (classSwitchVFXPrefab == null)
            return;

        GameObject effect = Instantiate(
            classSwitchVFXPrefab,
            position,
            Quaternion.identity
        );

        Destroy(effect, switchVFXLifetime);
    }

    private PlayerClassType GetClassTypeForMember(PartyMemberControlBridge member)
    {
        if (member == guardian)
            return PlayerClassType.Tank;

        if (member == toxicologist)
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