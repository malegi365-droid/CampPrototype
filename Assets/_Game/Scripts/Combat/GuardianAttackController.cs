using UnityEngine;

public class GuardianAttackController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator guardianAnimator;

    [Header("Combo Timing")]
    [SerializeField] private float comboResetTime = 1.1f;
    [SerializeField] private float inputCooldown = 0.2f;
    [SerializeField] private float heavyAttackRecovery = 0.8f;

    [Header("Weapon Hitbox")]
    [SerializeField] private GuardianWeaponHitbox weaponHitbox;
    [SerializeField] private float lightAttackDamage = 30f;
    [SerializeField] private float heavyAttackDamage = 55f;
    [SerializeField] private float lightActiveTime = 0.35f;
    [SerializeField] private float heavyActiveTime = 0.5f;

    private int comboStep = 0;
    private float lastInputTime;
    private float nextInputTime;
    private float hitboxOffTime;

    private void Update()
    {
        if (Time.time - lastInputTime > comboResetTime)
            comboStep = 0;

        if (Input.GetMouseButtonDown(0))
            TryComboAttack();

        if (weaponHitbox != null && Time.time >= hitboxOffTime)
            weaponHitbox.Deactivate();
    }

    private void TryComboAttack()
    {
        if (guardianAnimator == null || Time.time < nextInputTime)
            return;

        lastInputTime = Time.time;
        comboStep++;

        if (comboStep > 3)
            return;

        guardianAnimator.SetTrigger("Attack" + comboStep);

        bool isHeavy = comboStep == 3;
        float damage = isHeavy ? heavyAttackDamage : lightAttackDamage;
        float activeTime = isHeavy ? heavyActiveTime : lightActiveTime;

        if (weaponHitbox != null)
        {
            weaponHitbox.Activate(damage);
            hitboxOffTime = Time.time + activeTime;
        }

        if (comboStep == 3)
        {
            nextInputTime = Time.time + heavyAttackRecovery;
            comboStep = 0;
        }
        else
        {
            nextInputTime = Time.time + inputCooldown;
        }
    }
}