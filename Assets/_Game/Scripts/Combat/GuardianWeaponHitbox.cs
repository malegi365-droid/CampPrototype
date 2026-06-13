using System.Collections.Generic;
using UnityEngine;

public class GuardianWeaponHitbox : MonoBehaviour
{
    [Header("Owner / Filtering")]
    [SerializeField] private Transform ownerRoot;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Impact FX")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private GameObject hitSparkEffectPrefab;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float impactVolume = 1f;

    [Header("Hit Reaction")]
    [SerializeField] private float lightKnockbackStrength = 0.7f;
    [SerializeField] private float heavyKnockbackStrength = 1.7f;
    [SerializeField] private float lightStaggerDuration = 0.18f;
    [SerializeField] private float heavyStaggerDuration = 0.32f;

    [Header("Hit Stop")]
    [SerializeField] private bool enableHitStop = true;
    [SerializeField] private float lightHitStopDuration = 0.025f;
    [SerializeField] private float heavyHitStopDuration = 0.045f;

    private Collider hitboxCollider;
    private float damage;
    private bool isActive;
    private bool isHeavyAttack;

    private readonly HashSet<HealthController> damagedThisSwing = new();

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
        }
    }

    public void Activate(float newDamage)
    {
        damage = newDamage;
        isHeavyAttack = damage >= 50f;
        damagedThisSwing.Clear();

        isActive = true;

        if (hitboxCollider != null)
            hitboxCollider.enabled = true;
    }

    public void Deactivate()
    {
        isActive = false;

        if (hitboxCollider != null)
            hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;

        if (ownerRoot != null && other.transform.IsChildOf(ownerRoot))
            return;

        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        HealthController health = other.GetComponentInParent<HealthController>();

        if (health == null || damagedThisSwing.Contains(health))
            return;

        damagedThisSwing.Add(health);
        health.TakeDamage(damage);

        SpawnImpactEffects(other);
        ApplyCombatPolish(other.transform);
    }

    private void SpawnImpactEffects(Collider other)
    {
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, hitPoint, Quaternion.identity);

        if (hitSparkEffectPrefab != null)
            Instantiate(hitSparkEffectPrefab, hitPoint, Quaternion.identity);

        if (impactSound != null)
            AudioSource.PlayClipAtPoint(impactSound, hitPoint, impactVolume);
    }

    private void ApplyCombatPolish(Transform hitTransform)
    {
        EnemyHitFlash hitFlash = hitTransform.GetComponentInParent<EnemyHitFlash>();
        if (hitFlash == null)
            hitFlash = hitTransform.GetComponentInChildren<EnemyHitFlash>();

        if (hitFlash != null)
            hitFlash.TriggerFlash();

        EnemyHitReactionController reaction = hitTransform.GetComponentInParent<EnemyHitReactionController>();
        if (reaction == null)
            reaction = hitTransform.GetComponentInChildren<EnemyHitReactionController>();

        if (reaction != null)
        {
            Vector3 direction = hitTransform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f && ownerRoot != null)
            {
                direction = hitTransform.position - ownerRoot.position;
                direction.y = 0f;
            }

            if (direction.sqrMagnitude <= 0.001f)
                direction = transform.forward;

            direction.Normalize();

            reaction.ApplyHitReaction(
                direction,
                isHeavyAttack ? heavyKnockbackStrength : lightKnockbackStrength,
                isHeavyAttack ? heavyStaggerDuration : lightStaggerDuration,
                false
            );
        }

        if (enableHitStop && HitStopManager.Instance != null)
        {
            HitStopManager.Instance.DoHitStop(
                isHeavyAttack ? heavyHitStopDuration : lightHitStopDuration
            );
        }
    }
}