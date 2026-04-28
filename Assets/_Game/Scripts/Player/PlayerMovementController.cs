using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

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

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleMouseFacing()
    {
        if (aimCamera == null)
            return;

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        Ray ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

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