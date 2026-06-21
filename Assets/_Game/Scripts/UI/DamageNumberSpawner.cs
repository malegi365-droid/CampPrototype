using UnityEngine;
using UnityEngine.UI;

public class DamageNumberSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Camera worldCamera;

    [Header("Positioning")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.8f, 0f);

    [Header("Debug")]
    [SerializeField] private bool logSpawns = true;
    [SerializeField] private bool pressTToTest = true;

    private static DamageNumberSpawner instance;
    private Canvas runtimeCanvas;

    private void Awake()
    {
        instance = this;

        if (worldCamera == null)
            worldCamera = Camera.main;

        runtimeCanvas = CreateRuntimeCanvas();
    }

    private void Update()
    {
        if (pressTToTest && Input.GetKeyDown(KeyCode.T))
        {
            ShowHealing(Vector3.zero, 5f);
        }
    }

    private Canvas CreateRuntimeCanvas()
    {
        GameObject canvasObject = new GameObject("RuntimeDamageNumberCanvas");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        DontDestroyOnLoad(canvasObject);

        return canvas;
    }

    public static void ShowDamage(Vector3 worldPosition, float amount, bool crit = false)
    {
        if (instance == null)
        {
            Debug.LogWarning("[DamageNumberSpawner] No instance found.");
            return;
        }

        instance.SpawnNumber(worldPosition, amount, crit, false);
    }

    public static void ShowHealing(Vector3 worldPosition, float amount)
    {
        if (instance == null)
        {
            Debug.LogWarning("[DamageNumberSpawner] No instance found.");
            return;
        }

        instance.SpawnNumber(worldPosition, amount, false, true);
    }

    private void SpawnNumber(Vector3 worldPosition, float amount, bool crit, bool healing)
    {
        if (damageNumberPrefab == null)
        {
            Debug.LogWarning("[DamageNumberSpawner] Missing Damage Number Prefab.");
            return;
        }

        if (runtimeCanvas == null)
            runtimeCanvas = CreateRuntimeCanvas();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (worldCamera == null)
        {
            Debug.LogWarning("[DamageNumberSpawner] Missing world camera.");
            return;
        }

        Vector3 screenPosition =
            worldCamera.WorldToScreenPoint(worldPosition + worldOffset);

        if (screenPosition.z < 0f)
        {
            Debug.LogWarning("[DamageNumberSpawner] Number was behind camera.");
            return;
        }

        GameObject popup = Instantiate(
            damageNumberPrefab,
            runtimeCanvas.transform
        );

        popup.transform.position = screenPosition;

        DamageNumberPopup popupScript =
            popup.GetComponent<DamageNumberPopup>();

        if (popupScript != null)
            popupScript.Initialize(amount, crit, healing);
        else
            Debug.LogWarning("[DamageNumberSpawner] Prefab missing DamageNumberPopup script.");

        if (logSpawns)
        {
            string type = healing ? "Healing" : crit ? "Crit" : "Damage";
            Debug.Log($"[DamageNumberSpawner] Spawned {type} number: {amount}");
        }
    }
}