using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float walkSpeed = 3.5f;

    [Header("Overcharge Movement")]
    [SerializeField] private DPSProjectileFireController projectileFireController;
    [SerializeField] private float overchargeMoveSpeedMultiplier = 1.25f;

    [Header("HUD")]
    [SerializeField] private DPSAbilityHUDController abilityHUD;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.16f;
    [SerializeField] private float dashCooldown = 2.5f;
    [SerializeField] private bool allowDash = true;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string speedParameterName = "Speed";
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
            projectileFireController = GetComponent<DPSProjectileFireController>();

        if (abilityHUD == null)
            abilityHUD = FindAnyObjectByType<DPSAbilityHUDController>();
    }

    private void Update()
    {
        HandleDashInput();

        if (!isDashing)
        {
            HandleMovement();
            HandleMouseFacing();
        }
    }

    private void HandleMovement()
    {
        Vector3 move = GetMovementInput();

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        if (move.sqrMagnitude > 0.001f)
            lastMoveDirection = move.normalized;

        Keyboard kb = Keyboard.current;
        bool isWalking = false;

        if (kb != null)
            isWalking =
                kb.leftShiftKey.isPressed ||
                kb.rightShiftKey.isPressed;

        float currentSpeed =
            isWalking ? walkSpeed : runSpeed;

        if (IsOvercharged())
            currentSpeed *= overchargeMoveSpeedMultiplier;

        float animationSpeed = 0f;

        if (move.sqrMagnitude > 0.001f)
            animationSpeed = isWalking ? 0.5f : 1f;

        if (characterAnimator != null)
        {
            characterAnimator.SetFloat(
                speedParameterName,
                animationSpeed,
                animationDampTime,
                Time.deltaTime
            );
        }

        controller.Move(
            move * currentSpeed * Time.deltaTime
        );
    }

    private bool IsOvercharged()
    {
        return projectileFireController != null &&
               projectileFireController.IsOverchargeActive();
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

    private void HandleDashInput()
    {
        if (!allowDash || isDashing)
            return;

        Keyboard kb = Keyboard.current;

        if (kb == null)
            return;

        if (kb.spaceKey.wasPressedThisFrame &&
            Time.time >= nextDashTime)
        {
            Vector3 dashDirection = GetMovementInput();

            if (dashDirection.sqrMagnitude > 1f)
                dashDirection.Normalize();

            if (dashDirection.sqrMagnitude <= 0.001f)
                dashDirection = transform.forward;

            StartCoroutine(
                DashRoutine(dashDirection.normalized)
            );

            nextDashTime =
                Time.time + dashCooldown;

            if (abilityHUD != null)
                abilityHUD.TriggerDashCooldown();
        }
    }

    private IEnumerator DashRoutine(
        Vector3 dashDirection
    )
    {
        isDashing = true;

        if (characterAnimator != null &&
            !string.IsNullOrWhiteSpace(dashTriggerName))
        {
            characterAnimator.SetTrigger(
                dashTriggerName
            );
        }

        float elapsed = 0f;
        float dashSpeed =
            dashDistance / dashDuration;

        while (elapsed < dashDuration)
        {
            controller.Move(
                dashDirection *
                dashSpeed *
                Time.deltaTime
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        isDashing = false;
    }

    private void HandleMouseFacing()
    {
        if (aimCamera == null)
            return;

        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Ray ray =
            aimCamera.ScreenPointToRay(
                mouse.position.ReadValue()
            );

        Plane groundPlane =
            new Plane(Vector3.up, Vector3.zero);

        if (!groundPlane.Raycast(ray, out float enter))
            return;

        Vector3 point = ray.GetPoint(enter);

        Vector3 direction =
            point - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );
    }
}