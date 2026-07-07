using UnityEngine;
using System.Collections;

public class EnemyHitReactionController : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.14f;
    [SerializeField] private float knockbackSpeed = 10f;

    [Range(0f, 1f)]
    [SerializeField] private float knockbackResistance = 0f;

    [Header("Launch")]
    [SerializeField] private float launchDuration = 0.42f;
    [SerializeField] private float launchHorizontalSpeed = 8f;
    [SerializeField] private float launchHeight = 1.4f;
    [SerializeField] private AnimationCurve launchHeightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

    [Range(0f, 1f)]
    [SerializeField] private float launchResistance = 0f;

    [SerializeField] private bool immuneToLaunch = false;

    [Header("Stagger")]
    [SerializeField] private float defaultStaggerDuration = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float staggerResistance = 0f;

    [SerializeField] private bool immuneToStagger = false;
    [SerializeField] private bool immuneToKnockback = false;

    [Header("Overcharge Reaction Boost")]
    [SerializeField] private float overchargeKnockbackMultiplier = 1.35f;
    [SerializeField] private float overchargeStaggerMultiplier = 1.25f;

    private Coroutine knockbackRoutine;
    private Coroutine staggerRoutine;
    private Coroutine launchRoutine;

    private EnemyAIController enemyAI;
    private EnemyRoamingController roaming;
    private AutoAttackController autoAttack;
    private EnemyAnimationBridge animationBridge;
    private CharacterController characterController;

    private int controlLocks = 0;

    public bool IsStaggered { get; private set; }
    public bool IsKnockedBack { get; private set; }
    public bool IsLaunched { get; private set; }
    public bool IsReacting => IsStaggered || IsKnockedBack || IsLaunched;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIController>();
        roaming = GetComponent<EnemyRoamingController>();
        autoAttack = GetComponent<AutoAttackController>();
        animationBridge = GetComponent<EnemyAnimationBridge>();
        characterController = GetComponent<CharacterController>();
    }

    public void ApplyKnockback(Vector3 direction, float strength)
    {
        ApplyKnockback(direction, strength, false);
    }

    public void ApplyKnockback(Vector3 direction, float strength, bool isOverchargeHit)
    {
        if (immuneToKnockback)
            return;

        float boostedStrength = isOverchargeHit
            ? strength * overchargeKnockbackMultiplier
            : strength;

        float finalStrength = boostedStrength * (1f - knockbackResistance);

        if (finalStrength <= 0.01f)
            return;

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackRoutine(direction, finalStrength));
    }

    public void ApplyLaunch(Vector3 direction, float strength = 1f)
    {
        if (immuneToLaunch)
            return;

        float finalStrength = strength * (1f - launchResistance);

        if (finalStrength <= 0.01f)
            return;

        if (launchRoutine != null)
            StopCoroutine(launchRoutine);

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;

            if (IsKnockedBack)
            {
                IsKnockedBack = false;
                RemoveControlLock();
            }
        }

        launchRoutine = StartCoroutine(LaunchRoutine(direction, finalStrength));
    }

    public void ApplyStagger(float duration)
    {
        ApplyStagger(duration, false);
    }

    public void ApplyStagger(float duration, bool isOverchargeHit)
    {
        if (immuneToStagger)
            return;

        float selectedDuration = duration > 0f ? duration : defaultStaggerDuration;

        if (isOverchargeHit)
            selectedDuration *= overchargeStaggerMultiplier;

        float finalDuration = selectedDuration * (1f - staggerResistance);

        if (finalDuration <= 0.03f)
            return;

        if (staggerRoutine != null)
            StopCoroutine(staggerRoutine);

        staggerRoutine = StartCoroutine(StaggerRoutine(finalDuration));
    }

    public void ApplyHitReaction(Vector3 direction, float knockbackStrength, float staggerDuration, bool isOverchargeHit = false)
    {
        if (animationBridge != null)
            animationBridge.PlayHit();

        ApplyKnockback(direction, knockbackStrength, isOverchargeHit);
        ApplyStagger(staggerDuration, isOverchargeHit);
    }

    public void ApplyLaunchReaction(Vector3 direction, float launchStrength, float staggerDuration)
    {
        if (animationBridge != null)
            animationBridge.PlayHit();

        ApplyLaunch(direction, launchStrength);
        ApplyStagger(staggerDuration);
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float strength)
    {
        IsKnockedBack = true;
        AddControlLock();

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();

            float elapsed = 0f;
            float speed = knockbackSpeed * strength;

            while (elapsed < knockbackDuration)
            {
                MoveEnemy(direction * speed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        IsKnockedBack = false;
        RemoveControlLock();
        knockbackRoutine = null;
    }

    private IEnumerator LaunchRoutine(Vector3 direction, float strength)
    {
        IsLaunched = true;
        AddControlLock();

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            direction = transform.forward;

        direction.Normalize();

        Vector3 startPosition = transform.position;

        float elapsed = 0f;
        float previousHeight = 0f;
        float horizontalSpeed = launchHorizontalSpeed * strength;
        float height = launchHeight * strength;

        while (elapsed < launchDuration)
        {
            elapsed += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsed / launchDuration);
            float currentHeight = launchHeightCurve.Evaluate(normalizedTime) * height;
            float heightDelta = currentHeight - previousHeight;
            previousHeight = currentHeight;

            Vector3 horizontalMove = direction * horizontalSpeed * Time.deltaTime;
            Vector3 verticalMove = Vector3.up * heightDelta;

            MoveEnemy(horizontalMove + verticalMove);

            yield return null;
        }

        Vector3 finalPosition = transform.position;
        finalPosition.y = startPosition.y;
        transform.position = finalPosition;

        IsLaunched = false;
        RemoveControlLock();
        launchRoutine = null;
    }

    private IEnumerator StaggerRoutine(float duration)
    {
        IsStaggered = true;
        AddControlLock();

        yield return new WaitForSeconds(duration);

        IsStaggered = false;
        RemoveControlLock();
        staggerRoutine = null;
    }

    private void MoveEnemy(Vector3 movement)
    {
        if (characterController != null && characterController.enabled)
            characterController.Move(movement);
        else
            transform.position += movement;
    }

    private void AddControlLock()
    {
        controlLocks++;
        SetEnemyControl(false);
    }

    private void RemoveControlLock()
    {
        controlLocks = Mathf.Max(0, controlLocks - 1);

        if (controlLocks == 0)
            SetEnemyControl(true);
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