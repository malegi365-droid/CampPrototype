using System;
using UnityEngine;

[RequireComponent(typeof(UnitStats))]
public class HealthController : MonoBehaviour, IDamageable
{
    private UnitStats stats;
    private bool dead = false;

    public event Action<HealthController> OnDied;
    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
        ResetHealth();
    }

    public void TakeDamage(float amount, UnitStats sourceStats = null)
    {
        if (dead) return;

        float reducedDamage = Mathf.Max(1f, amount - stats.defense);
        stats.currentHP = Mathf.Max(0f, stats.currentHP - reducedDamage);

        OnHealthChanged?.Invoke(stats.currentHP, stats.maxHP);

        if (stats.currentHP <= 0f)
        {
            Die();
        }
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

        // Instead of deactivating the whole object here,
        // let other systems decide what to do next.
        HideIfEnemy();
    }

    private void HideIfEnemy()
    {
        UnitStats unitStats = GetComponent<UnitStats>();
        if (unitStats != null && unitStats.role == UnitRole.Enemy)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                c.enabled = false;
            }
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

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = true;
        }

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