using TMPro;
using UnityEngine;

public class DamageNumberPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text damageText;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 0.8f;

    [Header("Movement")]
    [SerializeField] private float riseSpeed = 45f;
    [SerializeField] private float horizontalDrift = 18f;

    [Header("Scale")]
    [SerializeField] private float startScale = 1.45f;
    [SerializeField] private float endScale = 1f;
    [SerializeField] private float scaleLerpSpeed = 10f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color critColor = new Color(1f, 0.75f, 0.15f);

    private float timer;
    private Vector3 moveDirection;
    private Color currentColor;
    private bool isCrit;

    private void Awake()
    {
        if (damageText == null)
            damageText = GetComponent<TMP_Text>();

        moveDirection = new Vector3(
            Random.Range(-horizontalDrift, horizontalDrift),
            riseSpeed,
            0f
        );

        transform.localScale = Vector3.one * startScale;
    }

    public void Initialize(float damageAmount, bool crit = false)
    {
        isCrit = crit;

        if (damageText != null)
        {
            damageText.text = Mathf.CeilToInt(damageAmount).ToString();

            currentColor = isCrit ? critColor : normalColor;
            damageText.color = currentColor;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.localPosition += moveDirection * Time.deltaTime;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * endScale,
            Time.deltaTime * scaleLerpSpeed
        );

        float fadePercent = timer / lifetime;

        if (damageText != null)
        {
            Color fadeColor = currentColor;
            fadeColor.a = Mathf.Lerp(1f, 0f, fadePercent);
            damageText.color = fadeColor;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}