using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyWorldHPBarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthController targetHealth;
    [SerializeField] private UnitStats targetStats;

    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Follow")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private bool faceCamera = true;

    private Camera mainCamera;

    public void Initialize(HealthController health, UnitStats stats)
    {
        targetHealth = health;
        targetStats = stats;

        if (targetStats != null && nameText != null)
            nameText.text = targetStats.unitName;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (targetHealth == null || targetStats == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = targetHealth.transform.position + worldOffset;

        if (faceCamera && mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }

        float healthPercent =
            targetHealth.GetCurrentHP() / targetHealth.GetMaxHP();

        if (fillImage != null)
            fillImage.fillAmount = healthPercent;

        if (targetHealth.IsDead())
        {
            Destroy(gameObject);
        }
    }
}