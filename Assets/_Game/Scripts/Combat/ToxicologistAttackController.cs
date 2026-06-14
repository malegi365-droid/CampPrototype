using UnityEngine;

public class ToxicologistAttackController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator toxicologistAnimator;
    [SerializeField] private string attackTriggerName = "Attack";

    [Header("Attack Timing")]
    [SerializeField] private float attackCooldown = 0.45f;

    private float nextAttackTime;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (toxicologistAnimator == null)
            return;

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;
        toxicologistAnimator.SetTrigger(attackTriggerName);
    }
}