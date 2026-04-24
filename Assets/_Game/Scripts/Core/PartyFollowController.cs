using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PartyFollowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyControlManager partyControlManager;
    [SerializeField] private PartyMemberControlBridge myControlBridge;

    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 4f;
    [SerializeField] private float stopDistance = 2.25f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Formation Offset")]
    [Tooltip("Local offset from active member. Example: x = side offset, z = behind/ahead offset.")]
    [SerializeField] private Vector3 formationOffset = new Vector3(1.5f, 0f, -2f);

    [Header("Grounding")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedStickForce = -2f;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (myControlBridge == null)
            myControlBridge = GetComponent<PartyMemberControlBridge>();

        if (partyControlManager == null)
            partyControlManager = FindFirstObjectByType<PartyControlManager>();
    }

    private void Update()
    {
        if (partyControlManager == null || myControlBridge == null)
            return;

        if (myControlBridge.IsPlayerControlled)
            return;

        PartyMemberControlBridge activeMember = partyControlManager.CurrentMember;

        if (activeMember == null || activeMember == myControlBridge)
            return;

        FollowActiveMember(activeMember);
    }

    private void FollowActiveMember(PartyMemberControlBridge activeMember)
    {
        Transform activeTransform = activeMember.transform;

        Vector3 targetPosition =
            activeTransform.position +
            activeTransform.right * formationOffset.x +
            activeTransform.forward * formationOffset.z;

        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        ApplyGravityOnlyIfNeeded();

        if (distance <= stopDistance)
        {
            Vector3 gravityMove = new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;
            characterController.Move(gravityMove);
            return;
        }

        if (distance > followDistance)
        {
            Vector3 moveDirection = toTarget.normalized;

            Vector3 horizontalMove = moveDirection * moveSpeed * Time.deltaTime;
            Vector3 verticalMove = new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;

            characterController.Move(horizontalMove + verticalMove);

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
    }

    private void ApplyGravityOnlyIfNeeded()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }
}