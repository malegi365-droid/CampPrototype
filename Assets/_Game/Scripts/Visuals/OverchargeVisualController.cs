using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OverchargeVisualController : MonoBehaviour
{
    [Header("Glow Renderers")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Emission")]
    [SerializeField] private Color overchargeEmissionColor = Color.cyan;
    [SerializeField] private float overchargeEmissionIntensity = 3f;

    [Header("Optional Effects")]
    [SerializeField] private GameObject overchargeAuraObject;

    [Header("Camera FOV")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float normalFOV = 70f;
    [SerializeField] private float overchargeFOV = 82f;
    [SerializeField] private float fovTransitionSpeed = 6f;

    [Header("Post Processing")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float overchargeChromaticIntensity = 0.25f;
    [SerializeField] private float overchargeVignetteIntensity = 0.28f;
    [SerializeField] private float postFXTransitionSpeed = 6f;

    private MaterialPropertyBlock propertyBlock;
    private Coroutine fovRoutine;
    private Coroutine postFXRoutine;

    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    private float baseChromaticIntensity = 0f;
    private float baseVignetteIntensity = 0.18f;

    private static readonly int EmissionColorID =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (globalVolume == null)
            globalVolume = FindAnyObjectByType<Volume>();

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
            globalVolume.profile.TryGet(out vignette);

            if (chromaticAberration != null)
                baseChromaticIntensity = chromaticAberration.intensity.value;

            if (vignette != null)
                baseVignetteIntensity = vignette.intensity.value;
        }

        DisableOverchargeVisuals();
    }

    public void EnableOverchargeVisuals()
    {
        Color finalEmission =
            overchargeEmissionColor * overchargeEmissionIntensity;

        foreach (Renderer rend in targetRenderers)
        {
            if (rend == null) continue;

            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorID, finalEmission);
            rend.SetPropertyBlock(propertyBlock);
        }

        if (overchargeAuraObject != null)
            overchargeAuraObject.SetActive(true);

        StartFOVTransition(overchargeFOV);
        StartPostFXTransition(overchargeChromaticIntensity, overchargeVignetteIntensity);
    }

    public void DisableOverchargeVisuals()
    {
        foreach (Renderer rend in targetRenderers)
        {
            if (rend == null) continue;

            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorID, Color.black);
            rend.SetPropertyBlock(propertyBlock);
        }

        if (overchargeAuraObject != null)
            overchargeAuraObject.SetActive(false);

        StartFOVTransition(normalFOV);
        StartPostFXTransition(baseChromaticIntensity, baseVignetteIntensity);
    }

    private void StartFOVTransition(float targetFOV)
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null) return;

        if (fovRoutine != null)
            StopCoroutine(fovRoutine);

        fovRoutine = StartCoroutine(FOVRoutine(targetFOV));
    }

    private IEnumerator FOVRoutine(float targetFOV)
    {
        while (targetCamera != null &&
               Mathf.Abs(targetCamera.fieldOfView - targetFOV) > 0.1f)
        {
            targetCamera.fieldOfView = Mathf.Lerp(
                targetCamera.fieldOfView,
                targetFOV,
                Time.deltaTime * fovTransitionSpeed
            );

            yield return null;
        }

        if (targetCamera != null)
            targetCamera.fieldOfView = targetFOV;

        fovRoutine = null;
    }

    private void StartPostFXTransition(float targetChromatic, float targetVignette)
    {
        if (postFXRoutine != null)
            StopCoroutine(postFXRoutine);

        postFXRoutine = StartCoroutine(PostFXRoutine(targetChromatic, targetVignette));
    }

    private IEnumerator PostFXRoutine(float targetChromatic, float targetVignette)
    {
        while (true)
        {
            bool chromaticDone = true;
            bool vignetteDone = true;

            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(
                    chromaticAberration.intensity.value,
                    targetChromatic,
                    Time.deltaTime * postFXTransitionSpeed
                );

                chromaticDone =
                    Mathf.Abs(chromaticAberration.intensity.value - targetChromatic) <= 0.01f;
            }

            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(
                    vignette.intensity.value,
                    targetVignette,
                    Time.deltaTime * postFXTransitionSpeed
                );

                vignetteDone =
                    Mathf.Abs(vignette.intensity.value - targetVignette) <= 0.01f;
            }

            if (chromaticDone && vignetteDone)
                break;

            yield return null;
        }

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = targetChromatic;

        if (vignette != null)
            vignette.intensity.value = targetVignette;

        postFXRoutine = null;
    }
}