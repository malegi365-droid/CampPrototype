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
        stats.currentHP = Mathf.Clamp(stats.currentHP, 0f, stats.maxHP);
        if (stats.currentHP <= 0f)
        {
            stats.currentHP = stats.maxHP;
        }
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
        // For prototype:
        // Option 1: gameObject.SetActive(false);
        // Option 2: Destroy(gameObject, 1.5f);
        gameObject.SetActive(false);
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