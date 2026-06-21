using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float walkSpeed = 3.5f;

    [Header("Visual Aiming")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float visualRotationSpeed = 18f;
    [SerializeField] private float visualYawOffset = 0f;

    [Header("Overcharge Movement")]
    [SerializeField] private RangerProjectileFireController projectileFireController;
    [SerializeField] private float overchargeMoveSpeedMultiplier = 1.25f;

    [Header("HUD")]
    [SerializeField] private RangerAbilityHUDController abilityHUD;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.16f;
    [SerializeField] private float dashCooldown = 2.5f;
    [SerializeField] private bool allowDash = true;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private string moveXParameterName = "MoveX";
    [SerializeField] private string moveYParameterName = "MoveY";
    [SerializeField] private float animationDampTime = 0.1f;
    [SerializeField] private string dashTriggerName = "Dash";

    [Header("Aiming")]
    [SerializeField] private Camera aimCamera;

    private CharacterController controller;
    private Vector3 lastMoveDirection = Vector3.forward;

    private bool isDashing = false;
    private float nextDashTime = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (projectileFireController == null)
            projectileFireController = GetComponent<RangerProjectileFireController>();

        if (abilityHUD == null)
            abilityHUD = FindAnyObjectByType<RangerAbilityHUDController>();

        if (characterAnimator == null)
            characterAnimator = GetComponentInChildren<Animator>();

        if (visualRoot == null && characterAnimator != null)
            visualRoot = characterAnimator.transform;
    }

    private void Update()
    {
        HandleDashInput();

        if (!isDashing)
        {
            HandleMovement();
            HandleVisualMouseFacing();
        }
    }

    private void HandleMovement()
    {
        Vector3 worldMove = GetMovementInput();

        if (worldMove.sqrMagnitude > 1f)
            worldMove.Normalize();

        bool isMoving = worldMove.sqrMagnitude > 0.001f;

        if (isMoving)
            lastMoveDirection = worldMove.normalized;

        bool isWalking = IsWalkHeld();
        float currentSpeed = isWalking ? walkSpeed : runSpeed;

        if (IsOvercharged())
            currentSpeed *= overchargeMoveSpeedMultiplier;

        // World/map movement. This does NOT depend on facing direction.
        controller.Move(worldMove * currentSpeed * Time.deltaTime);

        UpdateAnimation(worldMove, isMoving, currentSpeed);
    }

    private void UpdateAnimation(Vector3 worldMove, bool isMoving, float currentSpeed)
    {
        if (characterAnimator == null)
            return;

        float animatorSpeed = isMoving ? currentSpeed : 0f;

        Vector3 relativeMove = Vector3.zero;

        if (visualRoot != null && isMoving)
            relativeMove = visualRoot.InverseTransformDirection(worldMove);

        relativeMove.y = 0f;

        if (relativeMove.sqrMagnitude > 1f)
            relativeMove.Normalize();

        characterAnimator.SetFloat(
            speedParameterName,
            animatorSpeed,
            animationDampTime,
            Time.deltaTime
        );

        characterAnimator.SetFloat(
            moveXParameterName,
            relativeMove.x,
            animationDampTime,
            Time.deltaTime
        );

        characterAnimator.SetFloat(
            moveYParameterName,
            relativeMove.z,
            animationDampTime,
            Time.deltaTime
        );
    }

    private Vector3 GetMovementInput()
    {
        Vector3 move = Vector3.zero;
        Keyboard kb = Keyboard.current;

        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)
                move += Vector3.forward;

            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)
                move += Vector3.back;

            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
                move += Vector3.left;

            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
                move += Vector3.right;
        }

        return move;
    }

    private void HandleVisualMouseFacing()
    {
        if (visualRoot == null || aimCamera == null)
            return;

        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Ray ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (!groundPlane.Raycast(ray, out float enter))
            return;

        Vector3 point = ray.GetPoint(enter);
        Vector3 direction = point - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized, Vector3.up) *
            Quaternion.Euler(0f, visualYawOffset, 0f);

        visualRoot.rotation = Quaternion.Slerp(
            visualRoot.rotation,
            targetRotation,
            visualRotationSpeed * Time.deltaTime
        );
    }

    private bool IsWalkHeld()
    {
        Keyboard kb = Keyboard.current;

        return kb != null &&
               (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
    }

    private bool IsOvercharged()
    {
        return projectileFireController != null &&
               projectileFireController.IsOverchargeActive();
    }

    private void HandleDashInput()
    {
        if (!allowDash || isDashing)
            return;

        Keyboard kb = Keyboard.current;

        if (kb == null)
            return;

        if (kb.spaceKey.wasPressedThisFrame && Time.time >= nextDashTime)
        {
            Vector3 dashDirection = GetMovementInput();

            if (dashDirection.sqrMagnitude > 1f)
                dashDirection.Normalize();

            if (dashDirection.sqrMagnitude <= 0.001f)
                dashDirection = lastMoveDirection;

            StartCoroutine(DashRoutine(dashDirection.normalized));

            nextDashTime = Time.time + dashCooldown;

            abilityHUD?.TriggerDashCooldown();
        }
    }

    private IEnumerator DashRoutine(Vector3 dashDirection)
    {
        isDashing = true;

        if (characterAnimator != null && !string.IsNullOrWhiteSpace(dashTriggerName))
            characterAnimator.SetTrigger(dashTriggerName);

        float elapsed = 0f;
        float dashSpeed = dashDistance / dashDuration;

        while (elapsed < dashDuration)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}