using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PartyFollowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyControlManager partyControlManager;
    [SerializeField] private PartyMemberControlBridge myControlBridge;

    [Header("Follow Settings")]
    [SerializeField] private float followStartDistance = 5f;
    [SerializeField] private float stopDistance = 2.5f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Prototype Ground Lock")]
    [SerializeField] private bool lockYPosition = true;
    [SerializeField] private float lockedYPosition = 1f;

    private CharacterController characterController;
    private bool isFollowing;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (myControlBridge == null)
            myControlBridge = GetComponent<PartyMemberControlBridge>();

        if (partyControlManager == null)
            partyControlManager = FindFirstObjectByType<PartyControlManager>();

        // 🔥 REMOVED: lockedYPosition override
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

        PartyMemberControlBridge leader = partyControlManager.CurrentMember;

        if (leader == null || leader == myControlBridge)
            return;

        FollowLeaderDirectly(leader);

        if (lockYPosition)
            ForceLockedY();
    }

    private void FollowLeaderDirectly(PartyMemberControlBridge leader)
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