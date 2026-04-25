using UnityEngine;

public class CameraFollowProxy : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 12f;
    [SerializeField] private float snapDistance = 20f;

    private Transform target;

    public void SetTarget(Transform newTarget, bool snapImmediately = false)
    {
        target = newTarget;

        if (target != null && snapImmediately)
            transform.position = target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance >= snapDistance)
        {
            transform.position = target.position;
            return;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            followSpeed * Time.deltaTime
        );
    }
}