using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float walkSpeed = 3.5f;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private float animationDampTime = 0.1f;

    [Header("Aiming")]
    [SerializeField] private Camera aimCamera;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (aimCamera == null)
            aimCamera = Camera.main;
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseFacing();
    }

    private void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        Keyboard kb = Keyboard.current;
        bool isWalking = false;

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

            isWalking = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
        }

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        float currentSpeed = isWalking ? walkSpeed : runSpeed;

        float animationSpeed = 0f;
        if (move.sqrMagnitude > 0.001f)
            animationSpeed = isWalking ? 0.5f : 1f;

        if (characterAnimator != null)
            characterAnimator.SetFloat(speedParameterName, animationSpeed, animationDampTime, Time.deltaTime);

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleMouseFacing()
    {
        if (aimCamera == null)
            return;

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        Ray ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (!groundPlane.Raycast(ray, out float enter))
            return;

        Vector3 point = ray.GetPoint(enter);
        Vector3 direction = point - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}