using UnityEngine;

[RequireComponent(typeof(UnitStats))]
public class AbilityController : MonoBehaviour
{
    private UnitStats stats;
    private TargetingController targetingController;

    [Header("Unlocks")]
    public bool hasHeavyShot = true;
    public bool hasFocusBreak = false;

    [Header("Cooldowns")]
    public float heavyShotCooldown = 8f;
    public float focusBreakCooldown = 15f;
    public float guardingStrikeCooldown = 6f;
    public float restoreCooldown = 4f;

    private float heavyShotTimer;
    private float focusBreakTimer;
    private float guardingStrikeTimer;
    private float restoreTimer;

    private void Awake()
    {
        stats = GetComponent<UnitStats>();
        targetingController = GetComponent<TargetingController>();
    }

    private void Update()
    {
        heavyShotTimer -= Time.deltaTime;
        focusBreakTimer -= Time.deltaTime;
        guardingStrikeTimer -= Time.deltaTime;
        restoreTimer -= Time.deltaTime;
    }

    public bool UseHeavyShot()
    {
        if (!hasHeavyShot || heavyShotTimer > 0f) return false;

        Transform target = targetingController != null ? targetingController.GetCurrentTarget() : null;
        if (target == null) return false;

        IDamageable damageable = target.GetComponent<IDamageable>();
        ThreatTable threatTable = target.GetComponent<ThreatTable>();
        if (damageable == null) return false;

        float damage = stats.attack * 1.8f;
        damageable.TakeDamage(damage, stats);

        if (threatTable != null)
        {
            threatTable.AddThreat(gameObject, damage * stats.threatMultiplier);
        }

        heavyShotTimer = heavyShotCooldown;
        Debug.Log($"{gameObject.name} used Heavy Shot on {target.name} for {damage}");
        return true;
    }

    public bool UseFocusBreak()
    {
        if (!hasFocusBreak || focusBreakTimer > 0f) return false;

        Transform target = targetingController != null ? targetingController.GetCurrentTarget() : null;
        if (target == null) return false;

        IDamageable damageable = target.GetComponent<IDamageable>();
        ThreatTable threatTable = target.GetComponent<ThreatTable>();
        if (damageable == null) return false;

        float damage = stats.attack * 2.5f;
        damageable.TakeDamage(damage, stats);

        if (threatTable != null)
        {
            threatTable.AddThreat(gameObject, damage * stats.threatMultiplier);
        }

        focusBreakTimer = focusBreakCooldown;
        Debug.Log($"{gameObject.name} used Focus Break on {target.name} for {damage}");
        return true;
    }

    public bool UseGuardingStrike(Transform target)
    {
        if (guardingStrikeTimer > 0f || target == null) return false;

        IDamageable damageable = target.GetComponent<IDamageable>();
        ThreatTable threatTable = target.GetComponent<ThreatTable>();
        if (damageable == null) return false;

        float damage = stats.attack * 1.2f;
        damageable.TakeDamage(damage, stats);

        if (threatTable != null)
        {
            float threatAmount = (damage + 8f) * stats.threatMultiplier;
            threatTable.AddThreat(gameObject, threatAmount);
        }

        guardingStrikeTimer = guardingStrikeCooldown;
        Debug.Log($"{gameObject.name} used Guarding Strike on {target.name}");
        return true;
    }

    public bool UseRestore(GameObject targetObj)
    {
        if (restoreTimer > 0f || targetObj == null) return false;

        IDamageable damageable = targetObj.GetComponent<IDamageable>();
        if (damageable == null) return false;

        float healAmount = stats.healPower * 1.5f;
        damageable.ReceiveHealing(healAmount);

        restoreTimer = restoreCooldown;
        Debug.Log($"{gameObject.name} cast Restore on {targetObj.name} for {healAmount}");
        return true;
    }

    public void UnlockFocusBreak()
    {
        hasFocusBreak = true;
    }

    public float GetLastRestoreHealAmount()
    {
        return stats.healPower * 1.5f;
    }

    // ---------- UI HELPERS ----------
    public bool HasHeavyShot()
    {
        return hasHeavyShot;
    }

    public bool HasFocusBreak()
    {
        return hasFocusBreak;
    }

    public float GetHeavyShotRemainingCooldown()
    {
        return Mathf.Max(0f, heavyShotTimer);
    }

    public float GetFocusBreakRemainingCooldown()
    {
        return Mathf.Max(0f, focusBreakTimer);
    }

    public float GetHeavyShotCooldown()
    {
        return heavyShotCooldown;
    }

    public float GetFocusBreakCooldown()
    {
        return focusBreakCooldown;
    }

    public bool IsHeavyShotReady()
    {
        return hasHeavyShot && heavyShotTimer <= 0f;
    }

    public bool IsFocusBreakReady()
    {
        return hasFocusBreak && focusBreakTimer <= 0f;
    }
}