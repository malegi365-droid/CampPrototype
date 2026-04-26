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

    [Header("Combat Behavior")]
    [SerializeField] private bool stopFollowingWhenTargetingEnemy = true;
    [SerializeField] private float disengageDelay = 1.5f;

    [Tooltip("If this unit is in combat but gets this far from the leader, it drops target and returns.")]
    [SerializeField] private float recallDistanceFromLeader = 10f;

    [Header("Prototype Ground Lock")]
    [SerializeField] private bool lockYPosition = true;
    [SerializeField] private float lockedYPosition = 0.5f;

    private CharacterController characterController;
    private bool isFollowing;

    private float lastCombatTime;
    private bool wasInCombat;

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
        wasInCombat = false;

        if (lockYPosition)
            ForceLockedY();
    }

    private void Update()
    {
        if (partyControlManager == null || myControlBridge == null)
            return;

        if (myControlBridge.IsPlayerControlled)
            return;

        PartyMemberControlBridge leader = partyControlManager.CurrentMember;

        if (leader == null || leader == myControlBridge)
            return;

        float distanceFromLeader = GetFlatDistanceTo(leader.transform);

        if (IsInCombat())
        {
            if (distanceFromLeader >= recallDistanceFromLeader)
            {
                RecallToLeader();
            }
            else
            {
                lastCombatTime = Time.time;
                wasInCombat = true;
                isFollowing = false;

                if (lockYPosition)
                    ForceLockedY();

                return;
            }
        }

        if (wasInCombat)
        {
            if (Time.time - lastCombatTime < disengageDelay)
            {
                if (lockYPosition)
                    ForceLockedY();

                return;
            }

            wasInCombat = false;
        }

        FollowLeader(leader);

        if (lockYPosition)
            ForceLockedY();
    }

    private bool IsInCombat()
    {
        if (!stopFollowingWhenTargetingEnemy)
            return false;

        if (targetingController == null)
            return false;

        Transform currentTarget = targetingController.GetCurrentTarget();

        if (currentTarget == null)
            return false;

        HealthController health = currentTarget.GetComponent<HealthController>();

        if (health != null && health.IsDead())
            return false;

        return currentTarget.gameObject.activeInHierarchy;
    }

    private void RecallToLeader()
    {
        if (targetingController != null)
            targetingController.ClearTarget();

        wasInCombat = false;
        isFollowing = true;
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

    private float GetFlatDistanceTo(Transform target)
    {
        Vector3 offset = target.position - transform.position;
        offset.y = 0f;
        return offset.magnitude;
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