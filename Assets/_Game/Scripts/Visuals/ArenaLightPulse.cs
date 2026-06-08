using UnityEngine;

public class ArenaLightPulse : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;

    [SerializeField]
    private Color emissionColor =
        new Color(0f, 1f, 1f);

    [SerializeField] private float minIntensity = 1.5f;
    [SerializeField] private float maxIntensity = 3f;

    [SerializeField] private float pulseSpeed = 1f;

    private Material material;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer != null)
            material = targetRenderer.material;
    }

    private void Update()
    {
        if (material == null)
            return;

        float pulse =
            Mathf.Lerp(
                minIntensity,
                maxIntensity,
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f
            );

        material.SetColor(
            "_EmissionColor",
            emissionColor * pulse
        );
    }
}