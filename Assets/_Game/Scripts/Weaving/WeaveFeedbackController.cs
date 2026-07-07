using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaveFeedbackController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private CanvasGroup textCanvasGroup;
    [SerializeField] private TMP_Text weaveHeaderText;
    [SerializeField] private TMP_Text weaveNameText;

    [Header("Screen Pulse")]
    [SerializeField] private Image screenPulseImage;
    [SerializeField] private float pulseMaxAlpha = 0.35f;

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 0.12f;
    [SerializeField] private float holdTime = 0.75f;
    [SerializeField] private float fadeOutTime = 0.45f;

    [Header("Scale Punch")]
    [SerializeField] private RectTransform textRoot;
    [SerializeField] private float startScale = 1.25f;
    [SerializeField] private float endScale = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip weaveAudioClip;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Coroutine feedbackRoutine;
    private bool isSubscribed;

    private void Awake()
    {
        HideImmediate();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnEnable()
    {
        StartCoroutine(SubscribeNextFrame());
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private IEnumerator SubscribeNextFrame()
    {
        yield return null;
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        if (AbilityWeaveManager.Instance == null)
        {
            if (debugLogs)
                Debug.LogWarning("[WeaveFeedbackController] AbilityWeaveManager.Instance not found yet.");

            return;
        }

        AbilityWeaveManager.Instance.OnWeaveTriggered += PlayWeaveFeedback;
        isSubscribed = true;

        if (debugLogs)
            Debug.Log("[WeaveFeedbackController] Subscribed to AbilityWeaveManager.");
    }

    private void Unsubscribe()
    {
        if (!isSubscribed)
            return;

        if (AbilityWeaveManager.Instance != null)
            AbilityWeaveManager.Instance.OnWeaveTriggered -= PlayWeaveFeedback;

        isSubscribed = false;
    }

    private void PlayWeaveFeedback(string weaveName)
    {
        if (debugLogs)
            Debug.Log($"[WeaveFeedbackController] Playing feedback for {weaveName}");

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(FeedbackRoutine(weaveName));
    }

    private IEnumerator FeedbackRoutine(string weaveName)
    {
        if (weaveHeaderText != null)
            weaveHeaderText.text = "WEAVE";

        if (weaveNameText != null)
            weaveNameText.text = weaveName;

        if (audioSource != null && weaveAudioClip != null)
            audioSource.PlayOneShot(weaveAudioClip);

        if (textRoot != null)
            textRoot.localScale = Vector3.one * startScale;

        float timer = 0f;

        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeInTime);

            SetTextAlpha(t);
            SetPulseAlpha(Mathf.Lerp(0f, pulseMaxAlpha, t));
            SetTextScale(Mathf.Lerp(startScale, endScale, t));

            yield return null;
        }

        SetTextAlpha(1f);
        SetPulseAlpha(pulseMaxAlpha);
        SetTextScale(endScale);

        yield return new WaitForSeconds(holdTime);

        timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeOutTime);

            SetTextAlpha(1f - t);
            SetPulseAlpha(Mathf.Lerp(pulseMaxAlpha, 0f, t));

            yield return null;
        }

        HideImmediate();
    }

    private void SetTextAlpha(float alpha)
    {
        if (textCanvasGroup != null)
            textCanvasGroup.alpha = alpha;
    }

    private void SetPulseAlpha(float alpha)
    {
        if (screenPulseImage == null)
            return;

        Color color = screenPulseImage.color;
        color.a = alpha;
        screenPulseImage.color = color;
    }

    private void SetTextScale(float scale)
    {
        if (textRoot != null)
            textRoot.localScale = Vector3.one * scale;
    }

    private void HideImmediate()
    {
        SetTextAlpha(0f);
        SetPulseAlpha(0f);
        SetTextScale(endScale);
    }
}