using UnityEngine;

public class HitFlashController : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    private Color originalColor;
    private Material material;
    private float flashTimer = 0f;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            material = targetRenderer.material;
            originalColor = material.color;
        }
    }

    private void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0f)
            {
                material.color = originalColor;
            }
        }
    }

    public void Flash()
    {
        if (material == null)
            return;

        material.color = flashColor;
        flashTimer = flashDuration;
    }
}