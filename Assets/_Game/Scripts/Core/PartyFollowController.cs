using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PartyFollowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyControlManager partyControlManager;
    [SerializeField] private PartyMemberControlBridge myControlBridge;
    [SerializeField] private TargetingController targetingController;

    [Header("Follow Settings")]
    [SerializeField] private float followStartDistance = 5f;
    [SerializeField] private float stopDistance = 2.5f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Combat-Aware Follow")]
    [Tooltip("If true, this unit will stop following while it has a current target.")]
    [SerializeField] private bool stopFollowingWhenTargetingEnemy = true;

    [Header("Prototype Ground Lock")]
    [SerializeField] private bool lockYPosition = true;
    [SerializeField] private float lockedYPosition = 0.5f;

    private CharacterController characterController;
    private bool isFollowing;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (myControlBridge == null)
            myControlBridge = GetComponent<PartyMemberControlBridge>();

        if (targetingController == null)
            targetingController = GetComponent<TargetingController>();

        if (partyControlManager == null)
            partyControlManager = FindAnyObjectByType<PartyControlManager>();
    }

    private void OnEnable()
    {
        isFollowing = false;

        if (lockYPosition)
            ForceLockedY();
    }

    private void Update()
    {
        if (partyControlManager == null || myControlBridge == null)
            return;

        if (myControlBridge.IsPlayerControlled)
            return;

        if (ShouldStopFollowingForCombat())
        {
            isFollowing = false;

            if (lockYPosition)
                ForceLockedY();

            return;
        }

        PartyMemberControlBridge leader = partyControlManager.CurrentMember;

        if (leader == null || leader == myControlBridge)
            return;

        FollowLeader(leader);

        if (lockYPosition)
            ForceLockedY();
    }

    private bool ShouldStopFollowingForCombat()
    {
        if (!stopFollowingWhenTargetingEnemy)
            return false;

        if (targetingController == null)
            return false;

        Transform currentTarget = targetingController.GetCurrentTarget();

        if (currentTarget == null)
            return false;

        HealthController targetHealth = currentTarget.GetComponent<HealthController>();

        if (targetHealth != null && targetHealth.IsDead())
            return false;

        return currentTarget.gameObject.activeInHierarchy;
    }

    private void FollowLeader(PartyMemberControlBridge leader)
    {
        Vector3 toLeader = leader.transform.position - transform.position;
        toLeader.y = 0f;

        float distance = toLeader.magnitude;

        if (!isFollowing && distance >= followStartDistance)
            isFollowing = true;

        if (isFollowing && distance <= stopDistance)
            isFollowing = false;

        if (!isFollowing)
            return;

        Vector3 moveDirection = toLeader.normalized;
        Vector3 move = moveDirection * moveSpeed * Time.deltaTime;

        characterController.Move(move);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void ForceLockedY()
    {
        if (Mathf.Abs(transform.position.y - lockedYPosition) < 0.001f)
            return;

        characterController.enabled = false;

        Vector3 pos = transform.position;
        pos.y = lockedYPosition;
        transform.position = pos;

        characterController.enabled = true;
    }
}