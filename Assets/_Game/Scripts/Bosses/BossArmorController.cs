using UnityEngine;

public class BossArmorController : MonoBehaviour
{
    [Header("Armor Settings")]
    [SerializeField] private bool armorActive = true;
    [SerializeField] private float armoredDamageMultiplier = 0.2f;

    [Header("Armor Break")]
    [SerializeField] private float armorBreakDuration = 4f;

    private float armorBreakTimer = 0f;

    private void Update()
    {
        if (!armorActive)
            return;

        if (armorBreakTimer > 0f)
        {
            armorBreakTimer -= Time.deltaTime;
        }
    }

    public float ModifyIncomingDamage(float damage)
    {
        if (!armorActive)
            return damage;

        if (IsArmorBroken())
            return damage;

        return damage * armoredDamageMultiplier;
    }

    public void BreakArmor()
    {
        if (!armorActive)
            return;

        armorBreakTimer = armorBreakDuration;

        Debug.Log($"[BossArmorController] Armor broken for {armorBreakDuration} seconds.");
    }

    public bool IsArmorBroken()
    {
        return armorBreakTimer > 0f;
    }
}