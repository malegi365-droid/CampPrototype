using UnityEngine;

public class EnemyMovementAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform trackedRoot;
    [SerializeField] private string speedParameter = "MoveSpeed";

    [SerializeField] private float smoothing = 8f;

    private Vector3 lastPosition;
    private float currentSpeed;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (trackedRoot == null)
            trackedRoot = transform;

        lastPosition = trackedRoot.position;
    }

    private void Update()
    {
        float rawSpeed =
            (trackedRoot.position - lastPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        currentSpeed =
            Mathf.Lerp(currentSpeed, rawSpeed, Time.deltaTime * smoothing);

        if (animator != null)
            animator.SetFloat(speedParameter, currentSpeed);

        lastPosition = trackedRoot.position;
    }
}