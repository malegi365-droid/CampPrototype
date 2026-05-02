using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UnlockFeedbackUI : MonoBehaviour
{
    public static UnlockFeedbackUI Instance;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI unlockText;
    [SerializeField] private float displayTime = 2f;

    [Header("Sound")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private float unlockSoundVolume = 1f;
    [SerializeField] private float soundDelay = 0.12f;

    [Header("Screen Flash")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private float flashMaxAlpha = 0.45f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;

        if (unlockText != null)
            unlockText.gameObject.SetActive(false);

        if (flashImage != null)
        {
            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;
            flashImage.gameObject.SetActive(false);
        }
    }

    public void ShowUnlock(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        if (unlockText != null)
        {
            unlockText.text = message;
            unlockText.gameObject.SetActive(true);
        }

        if (flashImage != null)
            StartCoroutine(FlashRoutine());

        if (soundDelay > 0f)
            yield return new WaitForSeconds(soundDelay);

        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayUISound(unlockSound, unlockSoundVolume);
            Debug.Log("[UnlockFeedbackUI] Played unlock sound through GameAudioManager.");
        }
        else
        {
            Debug.LogWarning("[UnlockFeedbackUI] No GameAudioManager found.");
        }

        yield return new WaitForSeconds(displayTime);

        if (unlockText != null)
            unlockText.gameObject.SetActive(false);
    }

    private IEnumerator FlashRoutine()
    {
        flashImage.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / flashDuration;

            float alpha = Mathf.Lerp(flashMaxAlpha, 0f, progress);

            Color color = flashImage.color;
            color.a = alpha;
            flashImage.color = color;

            yield return null;
        }

        Color finalColor = flashImage.color;
        finalColor.a = 0f;
        flashImage.color = finalColor;

        flashImage.gameObject.SetActive(false);
    }
}