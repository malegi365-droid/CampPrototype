using System.Collections;
using UnityEngine;

public class PoisonStatusEffect : MonoBehaviour
{
    [Header("Poison Settings")]
    [SerializeField] private float tickInterval = 0.5f;

    [Header("Visual Feedback")]
    [SerializeField] private Color poisonTintColor = new Color(0.2f, 1f, 0.2f, 1f);
    [SerializeField] private float tintDuration = 0.12f;
    [SerializeField] private GameObject poisonTickEffectPrefab;

    [Header("Debug")]
    [SerializeField] private bool logPoisonTicks = true;

    private HealthController health;
    private Renderer[] renderers;
    private Color[][] originalColors;
    private Coroutine poisonRoutine;
    private Coroutine tintRoutine;

    private void Awake()
    {
        health = GetComponent<HealthController>();

        if (health == null)
            health = GetComponentInParent<HealthController>();

        renderers = GetComponentsInChildren<Renderer>();
        CacheOriginalColors();
    }

    public void ApplyPoison(float duration, float damagePerTick)
    {
        if (health == null)
            return;

        if (poisonRoutine != null)
            StopCoroutine(poisonRoutine);

        poisonRoutine = StartCoroutine(PoisonRoutine(duration, damagePerTick));
    }

    private IEnumerator PoisonRoutine(float duration, float damagePerTick)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);

            if (health == null)
                yield break;

            health.TakeDamage(damagePerTick);

            PlayPoisonTickFeedback(damagePerTick);

            elapsed += tickInterval;
        }

        poisonRoutine = null;
        RestoreOriginalColors();
    }

    private void PlayPoisonTickFeedback(float damagePerTick)
    {
        if (logPoisonTicks)
            Debug.Log($"Poison tick: {damagePerTick} damage on {gameObject.name}");

        if (poisonTickEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 1f;
            Instantiate(poisonTickEffectPrefab, spawnPosition, Quaternion.identity);
        }

        if (tintRoutine != null)
            StopCoroutine(tintRoutine);

        tintRoutine = StartCoroutine(TintPulse());
    }

    private IEnumerator TintPulse()
    {
        ApplyTint(poisonTintColor);

        yield return new WaitForSeconds(tintDuration);

        RestoreOriginalColors();
        tintRoutine = null;
    }

    private void CacheOriginalColors()
    {
        originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;
            originalColors[i] = new Color[materials.Length];

            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j].HasProperty("_BaseColor"))
                    originalColors[i][j] = materials[j].GetColor("_BaseColor");
                else if (materials[j].HasProperty("_Color"))
                    originalColors[i][j] = materials[j].GetColor("_Color");
            }
        }
    }

    private void ApplyTint(Color tintColor)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;

            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j].HasProperty("_BaseColor"))
                    materials[j].SetColor("_BaseColor", tintColor);
                else if (materials[j].HasProperty("_Color"))
                    materials[j].SetColor("_Color", tintColor);
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (originalColors == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;

            for (int j = 0; j < materials.Length; j++)
            {
                if (j >= originalColors[i].Length)
                    continue;

                if (materials[j].HasProperty("_BaseColor"))
                    materials[j].SetColor("_BaseColor", originalColors[i][j]);
                else if (materials[j].HasProperty("_Color"))
                    materials[j].SetColor("_Color", originalColors[i][j]);
            }
        }
    }
}