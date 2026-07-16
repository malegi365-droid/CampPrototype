using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechniqueDiscoveryPresentation : MonoBehaviour
{
    public static TechniqueDiscoveryPresentation Instance
    {
        get;
        private set;
    }

    [Header("Root")]
    [SerializeField] private CanvasGroup presentationGroup;

    [Header("Pulse")]
    [SerializeField] private Image screenPulse;
    [SerializeField] private float pulseMaxAlpha = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip aiSystemInterrupt;
    [SerializeField] private AudioClip aiSystemArchive;
    [SerializeField] private AudioClip aiTypingTick;
    [SerializeField] private int typingSoundEveryCharacters = 3;
    [SerializeField] private float typingTickVolume = 0.16f;

    [Header("AI Glitch Jolt")]
    [SerializeField] private RectTransform joltRoot;
    [SerializeField] private float joltAmount = 2f;

    [Header("Text Groups")]
    [SerializeField] private CanvasGroup analysisGroup;
    [SerializeField] private CanvasGroup patternGroup;
    [SerializeField] private CanvasGroup creativityGroup;
    [SerializeField] private CanvasGroup cataloguedGroup;
    [SerializeField] private CanvasGroup techniqueNameGroup;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text analysisText;
    [SerializeField] private TMP_Text patternText;
    [SerializeField] private TMP_Text creativityText;
    [SerializeField] private TMP_Text cataloguedText;
    [SerializeField] private TMP_Text techniqueNameText;

    [Header("Timing")]
    [SerializeField] private float freezeTimeScale = 0.05f;
    [SerializeField] private float freezeRealSeconds = 0.08f;
    [SerializeField] private float postJoltPause = 0.12f;

    [SerializeField] private float pulseInTime = 0.06f;
    [SerializeField] private float pulseOutTime = 0.18f;

    [Header("Typewriter")]
    [SerializeField] private float characterDelay = 0.014f;
    [SerializeField] private string cursor = "█";

    [SerializeField] private float analysisPause = 0.18f;
    [SerializeField] private float quickLinePause = 0.045f;
    [SerializeField] private float preNamePause = 0.18f;

    [SerializeField] private float techniqueNameFadeInTime = 0.18f;
    [SerializeField] private float holdTime = 0.75f;
    [SerializeField] private float fadeOutTime = 0.28f;

    [Header("Default Discovery Text")]
    [SerializeField]
    private string defaultAnalysisText =
        "SIMULATION ANALYSIS...";

    [SerializeField]
    private string defaultPatternText =
        "Pattern divergence detected.";

    [SerializeField]
    private string defaultCreativityText =
        "Creativity threshold exceeded.";

    [SerializeField]
    private string defaultCataloguedText =
        "Combat Signature Archived";

    [Header("Volatile Reaction Text")]
    [SerializeField]
    private string volatileAnalysisText =
        "CHEMICAL ANALYSIS...";

    [SerializeField]
    private string volatilePatternText =
        "Volatile compound detected.";

    [SerializeField]
    private string volatileCreativityText =
        "Cross-class reaction confirmed.";

    [SerializeField]
    private string volatileCataloguedText =
        "Combat Signature Archived";

    [Header("Testing")]
    [SerializeField] private KeyCode testKey = KeyCode.T;
    [SerializeField] private bool allowKeyboardTest = true;

    private Coroutine activeRoutine;
    private float previousTimeScale = 1f;
    private Vector2 joltOriginalPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (joltRoot == null)
            joltRoot = transform as RectTransform;

        if (joltRoot != null)
        {
            joltOriginalPosition =
                joltRoot.anchoredPosition;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        HideImmediate();
    }

    private void Update()
    {
        if (!allowKeyboardTest)
            return;

        if (Input.GetKeyDown(testKey))
        {
            ShowTechniqueDiscovery(
                "Meteor Dive"
            );
        }
    }

    public void ShowTechniqueDiscovery(
        string techniqueName
    )
    {
        DiscoveryTextSet selectedText =
            GetTextSetForTechnique(
                techniqueName
            );

        ShowTechniqueDiscovery(
            techniqueName,
            selectedText.analysis,
            selectedText.pattern,
            selectedText.creativity,
            selectedText.catalogued
        );
    }

    public void ShowTechniqueDiscovery(
        string techniqueName,
        string analysisLine,
        string patternLine,
        string creativityLine,
        string cataloguedLine
    )
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            Time.timeScale = previousTimeScale;
        }

        activeRoutine = StartCoroutine(
            ShowRoutine(
                techniqueName,
                analysisLine,
                patternLine,
                creativityLine,
                cataloguedLine
            )
        );
    }

    private DiscoveryTextSet GetTextSetForTechnique(
        string techniqueName
    )
    {
        bool isVolatileReaction =
            !string.IsNullOrWhiteSpace(
                techniqueName
            ) &&
            techniqueName.Trim().Equals(
                "Volatile Reaction",
                System.StringComparison
                    .OrdinalIgnoreCase
            );

        if (isVolatileReaction)
        {
            return new DiscoveryTextSet(
                volatileAnalysisText,
                volatilePatternText,
                volatileCreativityText,
                volatileCataloguedText
            );
        }

        return new DiscoveryTextSet(
            defaultAnalysisText,
            defaultPatternText,
            defaultCreativityText,
            defaultCataloguedText
        );
    }

    private IEnumerator ShowRoutine(
        string techniqueName,
        string analysisLine,
        string patternLine,
        string creativityLine,
        string cataloguedLine
    )
    {
        previousTimeScale = Time.timeScale;

        SetAllAlpha(0f);
        ClearText();

        if (presentationGroup != null)
            presentationGroup.alpha = 1f;

        if (screenPulse != null)
        {
            StartCoroutine(
                FadeImageAlpha(
                    screenPulse,
                    0f,
                    pulseMaxAlpha,
                    pulseInTime
                )
            );
        }

        Time.timeScale = freezeTimeScale;

        PlaySound(aiSystemInterrupt);

        yield return JoltRoutine();

        yield return new WaitForSecondsRealtime(
            freezeRealSeconds
        );

        Time.timeScale = previousTimeScale;

        yield return new WaitForSecondsRealtime(
            postJoltPause
        );

        yield return TypeLine(
            analysisGroup,
            analysisText,
            analysisLine
        );

        yield return new WaitForSecondsRealtime(
            analysisPause
        );

        yield return TypeLine(
            patternGroup,
            patternText,
            patternLine
        );

        yield return new WaitForSecondsRealtime(
            quickLinePause
        );

        yield return TypeLine(
            creativityGroup,
            creativityText,
            creativityLine
        );

        yield return new WaitForSecondsRealtime(
            quickLinePause
        );

        yield return TypeLine(
            cataloguedGroup,
            cataloguedText,
            cataloguedLine
        );

        yield return new WaitForSecondsRealtime(
            preNamePause
        );

        PlaySound(aiSystemArchive);

        if (techniqueNameText != null)
        {
            techniqueNameText.text =
                string.IsNullOrWhiteSpace(
                    techniqueName
                )
                    ? string.Empty
                    : techniqueName.ToUpper();
        }

        yield return FadeCanvasGroup(
            techniqueNameGroup,
            0f,
            1f,
            techniqueNameFadeInTime,
            0f
        );

        yield return new WaitForSecondsRealtime(
            holdTime
        );

        if (screenPulse != null)
        {
            StartCoroutine(
                FadeImageAlpha(
                    screenPulse,
                    pulseMaxAlpha,
                    0f,
                    pulseOutTime
                )
            );
        }

        yield return FadeCanvasGroup(
            presentationGroup,
            1f,
            0f,
            fadeOutTime,
            0f
        );

        HideImmediate();

        activeRoutine = null;
    }

    private IEnumerator JoltRoutine()
    {
        if (joltRoot == null ||
            joltAmount <= 0f)
        {
            yield break;
        }

        Vector2 original =
            joltRoot.anchoredPosition;

        joltRoot.anchoredPosition =
            original -
            Vector2.right * joltAmount;

        yield return null;

        joltRoot.anchoredPosition =
            original +
            Vector2.right * joltAmount;

        yield return null;

        joltRoot.anchoredPosition =
            original;
    }

    private IEnumerator TypeLine(
        CanvasGroup group,
        TMP_Text textField,
        string fullText
    )
    {
        if (group == null ||
            textField == null)
        {
            yield break;
        }

        if (fullText == null)
            fullText = string.Empty;

        group.alpha = 1f;
        textField.text = cursor;

        for (
            int i = 0;
            i <= fullText.Length;
            i++
        )
        {
            textField.text =
                fullText.Substring(0, i) +
                cursor;

            if (i > 0 &&
                typingSoundEveryCharacters > 0 &&
                i % typingSoundEveryCharacters == 0)
            {
                PlaySound(
                    aiTypingTick,
                    typingTickVolume
                );
            }

            yield return new WaitForSecondsRealtime(
                characterDelay
            );
        }

        textField.text = fullText;
    }

    private void PlaySound(
        AudioClip clip,
        float volume = 1f
    )
    {
        if (audioSource == null ||
            clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            clip,
            volume
        );
    }

    private void ClearText()
    {
        if (analysisText != null)
            analysisText.text = "";

        if (patternText != null)
            patternText.text = "";

        if (creativityText != null)
            creativityText.text = "";

        if (cataloguedText != null)
            cataloguedText.text = "";

        if (techniqueNameText != null)
            techniqueNameText.text = "";
    }

    private void HideImmediate()
    {
        Time.timeScale = previousTimeScale;

        if (presentationGroup != null)
            presentationGroup.alpha = 0f;

        SetAllAlpha(0f);
        ClearText();

        if (screenPulse != null)
        {
            Color color = screenPulse.color;
            color.a = 0f;
            screenPulse.color = color;
        }

        if (joltRoot != null)
        {
            joltRoot.anchoredPosition =
                joltOriginalPosition;
        }
    }

    private void SetAllAlpha(float alpha)
    {
        if (analysisGroup != null)
            analysisGroup.alpha = alpha;

        if (patternGroup != null)
            patternGroup.alpha = alpha;

        if (creativityGroup != null)
            creativityGroup.alpha = alpha;

        if (cataloguedGroup != null)
            cataloguedGroup.alpha = alpha;

        if (techniqueNameGroup != null)
            techniqueNameGroup.alpha = alpha;
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup group,
        float from,
        float to,
        float duration,
        float delay
    )
    {
        if (group == null)
            yield break;

        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                delay
            );
        }

        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                duration <= 0f
                    ? 1f
                    : Mathf.Clamp01(
                        elapsed / duration
                    );

            group.alpha =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );

            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator FadeImageAlpha(
        Image image,
        float from,
        float to,
        float duration
    )
    {
        if (image == null)
            yield break;

        float elapsed = 0f;

        Color color = image.color;
        color.a = from;
        image.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                duration <= 0f
                    ? 1f
                    : Mathf.Clamp01(
                        elapsed / duration
                    );

            color.a =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );

            image.color = color;

            yield return null;
        }

        color.a = to;
        image.color = color;
    }

    private readonly struct DiscoveryTextSet
    {
        public readonly string analysis;
        public readonly string pattern;
        public readonly string creativity;
        public readonly string catalogued;

        public DiscoveryTextSet(
            string analysis,
            string pattern,
            string creativity,
            string catalogued
        )
        {
            this.analysis = analysis;
            this.pattern = pattern;
            this.creativity = creativity;
            this.catalogued = catalogued;
        }
    }
}