using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(UnitStats))]
public class HealthController : MonoBehaviour, IDamageable
{
    private UnitStats stats;
    private BossArmorController bossArmor;
    private EnemyHitFlash enemyHitFlash;
    private bool dead = false;

    public event Action<HealthController> OnDied;
    public event Action<float, float> OnHealthChanged;

    [Header("Death FX")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private Vector3 deathEffectOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float deathSoundVolume = 1f;
    [SerializeField] private bool shrinkEnemyOnDeath = true;
    [SerializeField] private float deathShrinkDuration = 0.45f;

    [Header("Damage Number Settings")]
    [SerializeField] private float temporaryCritThreshold = 20f;

    [Header("Player Damage Feedback")]
    [SerializeField] private bool enablePlayerDamageFeedback = true;
    [SerializeField] private bool logPlayerDamageFeedback = true;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
        bossArmor = GetComponent<BossArmorController>();
        enemyHitFlash = GetComponent<EnemyHitFlash>();

        ResetHealth();
    }

    public void TakeDamage(float amount, UnitStats sourceStats = null)
    {
        if (dead) return;

        float incomingDamage = amount;

        if (bossArmor != null)
            incomingDamage = bossArmor.ModifyIncomingDamage(incomingDamage);

        float reducedDamage = Mathf.Max(1f, incomingDamage - stats.defense);
        stats.currentHP = Mathf.Max(0f, stats.currentHP - reducedDamage);

        if (stats.role == UnitRole.Enemy)
        {
            bool crit = reducedDamage >= temporaryCritThreshold;

            DamageNumberSpawner.ShowDamage(
                transform.position,
                reducedDamage,
                crit
            );
        }

        TriggerEnemyHitFlash();
        TriggerDamageFeedback(reducedDamage);
        AddThreatFromDamage(sourceStats, reducedDamage);

        OnHealthChanged?.Invoke(stats.currentHP, stats.maxHP);

        if (stats.currentHP <= 0f)
            Die();
    }

    private void TriggerEnemyHitFlash()
    {
        if (stats == null || stats.role != UnitRole.Enemy)
            return;

        if (enemyHitFlash != null)
            enemyHitFlash.TriggerFlash();
    }

    private void AddThreatFromDamage(UnitStats sourceStats, float damageAmount)
    {
        if (sourceStats == null)
            return;

        if (stats == null || stats.role != UnitRole.Enemy)
            return;

        ThreatTable myThreatTable = GetComponent<ThreatTable>();

        if (myThreatTable == null)
            return;

        myThreatTable.AddThreat(sourceStats.gameObject, damageAmount);

        Debug.Log($"{gameObject.name} gained {damageAmount} threat toward {sourceStats.gameObject.name}");
    }

    private void TriggerDamageFeedback(float damageTaken)
    {
        if (!enablePlayerDamageFeedback)
            return;

        if (stats == null)
            return;

        if (stats.role == UnitRole.Enemy)
            return;

        HitFlashController flash = GetComponentInChildren<HitFlashController>();
        if (flash != null)
            flash.Flash();

        if (logPlayerDamageFeedback)
            Debug.Log($"{gameObject.name} took {damageTaken} damage. Player damage feedback triggered.");
    }

    public void ReceiveHealing(float amount)
    {
        if (dead) return;

        stats.currentHP = Mathf.Min(stats.maxHP, stats.currentHP + amount);
        OnHealthChanged?.Invoke(stats.currentHP, stats.maxHP);
    }

    public bool IsDead()
    {
        return dead;
    }

    private void Die()
    {
        if (dead) return;

        dead = true;
        OnDied?.Invoke(this);

        Debug.Log($"{gameObject.name} died.");

        SpawnDeathEffect();
        PlayDeathSound();
        HandleDeathCleanup();
    }

    private void HandleDeathCleanup()
    {
        if (stats != null && stats.role == UnitRole.Enemy)
        {
            DisableEnemyCollisionAndCombat();

            if (shrinkEnemyOnDeath)
                StartCoroutine(ShrinkAndHideEnemy());
            else
                HideIfEnemy();

            return;
        }

        gameObject.SetActive(false);
    }

    private void DisableEnemyCollisionAndCombat()
    {
        EnemyAIController enemyAI = GetComponent<EnemyAIController>();
        if (enemyAI != null)
            enemyAI.enabled = false;

        EnemyRoamingController roaming = GetComponent<EnemyRoamingController>();
        if (roaming != null)
            roaming.enabled = false;

        AutoAttackController autoAttack = GetComponent<AutoAttackController>();
        if (autoAttack != null)
            autoAttack.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
            c.enabled = false;
    }

    private IEnumerator ShrinkAndHideEnemy()
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < deathShrinkDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / deathShrinkDuration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
    }

    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null)
            return;

        Vector3 spawnPosition = transform.position + deathEffectOffset;
        Instantiate(deathEffectPrefab, spawnPosition, Quaternion.identity);
    }

    private void PlayDeathSound()
    {
        if (deathSound == null)
            return;

        Vector3 soundPosition = transform.position + deathEffectOffset;

        AudioSource.PlayClipAtPoint(
            deathSound,
            soundPosition,
            deathSoundVolume
        );
    }

    private void HideIfEnemy()
    {
        if (stats != null && stats.role == UnitRole.Enemy)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = false;

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
                c.enabled = false;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetHealth()
    {
        dead = false;
        stats.currentHP = stats.maxHP;

        transform.localScale = Vector3.one;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = true;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
            c.enabled = true;

        EnemyAIController enemyAI = GetComponent<EnemyAIController>();
        if (enemyAI != null)
            enemyAI.enabled = true;

        EnemyRoamingController roaming = GetComponent<EnemyRoamingController>();
        if (roaming != null)
            roaming.enabled = true;

        AutoAttackController autoAttack = GetComponent<AutoAttackController>();
        if (autoAttack != null)
            autoAttack.enabled = true;

        OnHealthChanged?.Invoke(stats.currentHP, stats.maxHP);
    }

    public float GetCurrentHP()
    {
        return stats.currentHP;
    }

    public float GetMaxHP()
    {
        return stats.maxHP;
    }

    public float GetHealthPercent()
    {
        if (stats.maxHP <= 0f) return 0f;
        return stats.currentHP / stats.maxHP;
    }
}