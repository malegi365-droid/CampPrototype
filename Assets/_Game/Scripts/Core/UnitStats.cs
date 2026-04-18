using UnityEngine;

public enum UnitRole
{
    PlayerDPS,
    Tank,
    Healer,
    Enemy
}

public class UnitStats : MonoBehaviour
{
    [Header("Identity")]
    public string unitName = "Unit";
    public UnitRole role = UnitRole.Enemy;
    public int level = 1;

    [Header("Core Stats")]
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float attack = 10f;
    public float defense = 5f;

    [Header("Combat")]
    public float attackInterval = 3f;
    public float attackRange = 2.5f;
    public float threatMultiplier = 1f;
    public float healPower = 10f;

    private void Awake()
    {
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
        if (currentHP <= 0f)
        {
            currentHP = maxHP;
        }
    }

    public void SetLevelStats(
        int newLevel,
        float newMaxHP,
        float newAttack,
        float newDefense,
        float newHealPower = 0f,
        float newThreatMultiplier = 1f)
    {
        level = newLevel;
        maxHP = newMaxHP;
        attack = newAttack;
        defense = newDefense;
        healPower = newHealPower;
        threatMultiplier = newThreatMultiplier;

        currentHP = maxHP;
    }
}