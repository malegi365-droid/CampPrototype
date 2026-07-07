using System.Collections;
using TMPro;
using UnityEngine;

public class WeaveFeedbackUI : MonoBehaviour
{
    [SerializeField] private TMP_Text weaveText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float showTime = 1.25f;

    private Coroutine currentRoutine;

    private void OnEnable()
    {
        if (AbilityWeaveManager.Instance != null)
        {
            AbilityWeaveManager.Instance.OnWeaveTriggered += ShowWeave;
        }
    }

    private void OnDisable()
    {
        if (AbilityWeaveManager.Instance != null)
        {
            AbilityWeaveManager.Instance.OnWeaveTriggered -= ShowWeave;
        }
    }

    private void ShowWeave(string weaveName)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowRoutine(weaveName));
    }

    private IEnumerator ShowRoutine(string weaveName)
    {
        weaveText.text = $"WEAVE\n{weaveName}";
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(showTime);

        canvasGroup.alpha = 0f;
    }
}