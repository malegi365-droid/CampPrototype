using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonCloudZone : MonoBehaviour
{
    [Header("Cloud Settings")]
    [SerializeField] private float cloudDuration = 6f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float poisonDuration = 4f;
    [SerializeField] private float poisonDamagePerTick = 5f;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;

    private readonly HashSet<PoisonStatusEffect> poisonedEnemies = new();

    public void Initialize(
        float duration,
        float interval,
        float poisonTime,
        float damagePerTick,
        LayerMask enemies
    )
    {
        cloudDuration = duration;
        tickInterval = interval;
        poisonDuration = poisonTime;
        poisonDamagePerTick = damagePerTick;
        enemyLayer = enemies;

        StartCoroutine(CloudRoutine());
        Destroy(gameObject, cloudDuration + 0.25f);
    }

    private IEnumerator CloudRoutine()
    {
        float elapsed = 0f;

        while (elapsed < cloudDuration)
        {
            ApplyPoisonToEnemiesInside();

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
    }

    private void ApplyPoisonToEnemiesInside()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            GetCloudRadius(),
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            PoisonStatusEffect poison =
                hit.GetComponentInParent<PoisonStatusEffect>();

            if (poison == null)
                continue;

            poison.ApplyPoison(poisonDuration, poisonDamagePerTick);

            poisonedEnemies.Add(poison);
        }
    }

    private float GetCloudRadius()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();

        if (sphere != null)
            return sphere.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);

        return 3f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GetCloudRadius());
    }
}