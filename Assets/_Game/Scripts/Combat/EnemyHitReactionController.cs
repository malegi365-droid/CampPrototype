using UnityEngine;
using System.Collections;

public class EnemyHitReactionController : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.12f;
    [SerializeField] private float knockbackSpeed = 8f;

    [Range(0f, 1f)]
    [SerializeField] private float knockbackResistance = 0f;

    [Header("Stagger")]
    [SerializeField] private float defaultStaggerDuration = 0.18f;

    [Range(0f, 1f)]
    [SerializeField] private float staggerResistance = 0f;

    [SerializeField] private bool immuneToStagger = false;
    [SerializeField] private bool immuneToKnockback = false;

    private Coroutine knockbackRoutine;
    private Coroutine staggerRoutine;

    private EnemyAIController enemyAI;
    private EnemyRoamingController roaming;
    private AutoAttackController autoAttack;

    public bool IsStaggered { get; private set; }

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIController>();
        roaming = GetComponent<EnemyRoamingController>();
        autoAttack = GetComponent<AutoAttackController>();
    }

    public void ApplyKnockback(Vector3 direction, float strength)
    {
        if (immuneToKnockback)
            return;

        float finalStrength = strength * (1f - knockbackResistance);

        if (finalStrength <= 0.01f)
            return;

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackRoutine(direction, finalStrength));
    }

    public void ApplyStagger(float duration)
    {
        if (immuneToStagger)
            return;

        float finalDuration = duration * (1f - staggerResistance);

        if (finalDuration <= 0.03f)
            return;

        if (staggerRoutine != null)
            StopCoroutine(staggerRoutine);

        staggerRoutine = StartCoroutine(StaggerRoutine(finalDuration));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float strength)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            yield break;

        direction.Normalize();

        float elapsed = 0f;
        float speed = knockbackSpeed * strength;

        while (elapsed < knockbackDuration)
        {
            transform.position += direction * speed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        knockbackRoutine = null;
    }

    private IEnumerator StaggerRoutine(float duration)
    {
        IsStaggered = true;
        SetEnemyControl(false);

        yield return new WaitForSeconds(duration > 0f ? duration : defaultStaggerDuration);

        SetEnemyControl(true);
        IsStaggered = false;
        staggerRoutine = null;
    }

    private void SetEnemyControl(bool enabled)
    {
        if (enemyAI != null)
            enemyAI.enabled = enabled;

        if (roaming != null)
            roaming.enabled = enabled;

        if (autoAttack != null)
            autoAttack.enabled = enabled;
    }
}