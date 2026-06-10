using UnityEngine;

public class PlayerAnimationBridge : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform movementRoot;

    private Vector3 lastPosition;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (movementRoot == null)
            movementRoot = transform;

        lastPosition = movementRoot.position;
    }

    private void Update()
    {
        if (animator == null || movementRoot == null)
            return;

        Vector3 delta = movementRoot.position - lastPosition;
        delta.y = 0f;

        float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.001f);
        lastPosition = movementRoot.position;

        animator.SetFloat("Speed", speed);
    }

    public void PlayAttack()
    {
        animator?.SetTrigger("Attack");
    }
}