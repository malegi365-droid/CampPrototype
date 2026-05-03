using UnityEngine;
using System.Collections;

public class BossSweepAttack : MonoBehaviour
{
    [Header("Sweep Settings")]
    [SerializeField] private float sweepCooldown = 3f;
    [SerializeField] private float sweepRange = 3.5f;
    [SerializeField] private float sweepDamage = 18f;
    [SerializeField] private float windupTime = 0.45f;

    [Header("Targeting")]
    [SerializeField] private LayerMask playerLayers;

    [Header("Debug")]
    [SerializeField] private bool logEvents = true;

    private bool isSweeping = false;
    private float lastSweepTime = -999f;

    private void Update()
    {
        if (isSweeping)
            return;

        if (Time.time - lastSweepTime < sweepCooldown)
            return;

        if (PlayerInRange())
        {
            StartCoroutine(SweepRoutine());
        }
    }

    private bool PlayerInRange()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            sweepRange,
            playerLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            HealthController health = hit.GetComponent<HealthController>();
            if (health == null)
                health = hit.GetComponentInParent<HealthController>();

            UnitStats stats = hit.GetComponent<UnitStats>();
            if (stats == null)
                stats = hit.GetComponentInParent<UnitStats>();

            if (health != null && !health.IsDead() && stats != null && stats.role != UnitRole.Enemy)
                return true;
        }

        return false;
    }

    private IEnumerator SweepRoutine()
    {
        isSweeping = true;
        lastSweepTime = Time.time;

        if (logEvents)
            Debug.Log("[BossSweepAttack] Sweep windup.");

        yield return new WaitForSeconds(windupTime);

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            sweepRange,
            playerLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            HealthController health = hit.GetComponent<HealthController>();
            if (health == null)
                health = hit.GetComponentInParent<HealthController>();

            UnitStats stats = hit.GetComponent<UnitStats>();
            if (stats == null)
                stats = hit.GetComponentInParent<UnitStats>();

            if (health == null || stats == null || health.IsDead())
                continue;

            if (stats.role == UnitRole.Enemy)
                continue;

            health.TakeDamage(sweepDamage);

            if (logEvents)
                Debug.Log("[BossSweepAttack] Sweep hit player.");

            break;
        }

        isSweeping = false;
    }
}