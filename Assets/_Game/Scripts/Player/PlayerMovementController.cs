using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
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
}