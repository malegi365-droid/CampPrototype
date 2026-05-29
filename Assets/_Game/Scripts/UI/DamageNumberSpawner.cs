using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private Camera worldCamera;

    [Header("Positioning")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.4f, 0f);

    private static DamageNumberSpawner instance;

    private void Awake()
    {
        instance = this;

        if (targetCanvas == null)
            targetCanvas = FindAnyObjectByType<Canvas>();

        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    public static void ShowDamage(
        Vector3 worldPosition,
        float amount,
        bool crit = false)
    {
        if (instance == null)
            return;

        instance.SpawnDamageNumber(
            worldPosition,
            amount,
            crit
        );
    }

    private void SpawnDamageNumber(
        Vector3 worldPosition,
        float amount,
        bool crit)
    {
        if (damageNumberPrefab == null)
            return;

        if (targetCanvas == null)
            return;

        if (worldCamera == null)
            return;

        Vector3 screenPosition =
            worldCamera.WorldToScreenPoint(
                worldPosition + worldOffset
            );

        GameObject popup = Instantiate(
            damageNumberPrefab,
            targetCanvas.transform
        );

        popup.transform.position = screenPosition;

        DamageNumberPopup popupScript =
            popup.GetComponent<DamageNumberPopup>();

        if (popupScript != null)
            popupScript.Initialize(amount, crit);
    }
}