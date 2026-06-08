using System.Collections;
using TMPro;
using UnityEngine;

public class CellResourceManager : MonoBehaviour
{
    public static CellResourceManager Instance;

    [Header("Resources")]
    [SerializeField] private int currentCells = 0;

    [Header("UI")]
    [SerializeField] private TMP_Text cellText;
    [SerializeField] private RectTransform cellTextTransform;

    [Header("UI Pulse")]
    [SerializeField] private float pulseScale = 1.25f;
    [SerializeField] private float pulseDuration = 0.1f;

    private Coroutine pulseRoutine;
    private Vector3 originalScale;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (cellTextTransform == null && cellText != null)
            cellTextTransform = cellText.GetComponent<RectTransform>();

        if (cellTextTransform != null)
            originalScale = cellTextTransform.localScale;

        UpdateUI(false);
    }

    public void AddCells(int amount)
    {
        currentCells += amount;

        Debug.Log($"Cells gained: {amount} | Total: {currentCells}");

        UpdateUI(true);
    }

    private void UpdateUI(bool pulse)
    {
        if (cellText != null)
            cellText.text = $"CELLS: {currentCells:00}";

        if (pulse)
            PulseCellsUI();
    }

    private void PulseCellsUI()
    {
        if (cellTextTransform == null)
            return;

        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        float elapsed = 0f;

        while (elapsed < pulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / pulseDuration);
            float scale = Mathf.Lerp(1f, pulseScale, t);

            cellTextTransform.localScale = originalScale * scale;

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < pulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / pulseDuration);
            float scale = Mathf.Lerp(pulseScale, 1f, t);

            cellTextTransform.localScale = originalScale * scale;

            yield return null;
        }

        cellTextTransform.localScale = originalScale;
        pulseRoutine = null;
    }
}