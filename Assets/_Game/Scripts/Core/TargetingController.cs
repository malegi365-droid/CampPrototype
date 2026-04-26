using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingController : MonoBehaviour
{
    [SerializeField] private Transform currentTarget;
    [SerializeField] private float targetSearchRadius = 12f;

    private void Update()
    {
        Keyboard kb = Keyboard.current;

        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            ClearTarget();
            Debug.Log($"{gameObject.name} cleared target.");
            return;
        }

        if (kb != null && kb.tabKey.wasPressedThisFrame)
        {
            TargetNearestEnemy();
        }

        if (currentTarget != null)
        {
            HealthController health = currentTarget.GetComponent<HealthController>();
            if (!currentTarget.gameObject.activeInHierarchy || (health != null && health.IsDead()))
            {
                currentTarget = null;
            }
        }
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;

        if (currentTarget != null)
            Debug.Log($"{gameObject.name} targeted {currentTarget.name}");
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    private void TargetNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, targetSearchRadius);

        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            UnitStats stats = hit.GetComponent<UnitStats>();
            HealthController health = hit.GetComponent<HealthController>();

            if (stats == null || health == null)
                continue;

            if (stats.role != UnitRole.Enemy)
                continue;

            if (health.IsDead())
                continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearest = hit.transform;
            }
        }

        if (nearest != null)
            SetTarget(nearest);
        else
            Debug.Log($"{gameObject.name} found no enemy to target.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetSearchRadius);
    }
}