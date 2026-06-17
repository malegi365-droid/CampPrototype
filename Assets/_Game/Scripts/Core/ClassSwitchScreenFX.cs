using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ClassSwitchScreenFX : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Volume postProcessVolume;

    [Header("Chromatic Aberration")]
    [SerializeField] private float chromaticIntensity = 0.75f;

    [Header("Lens Distortion")]
    [SerializeField] private float distortionIntensity = -0.25f;

    [Header("Timing")]
    [SerializeField] private float pulseDuration = 0.22f;

    [Header("Camera Shake")]
    [SerializeField] private CameraShakeController cameraShake;
    [SerializeField] private float shakeDuration = 0.12f;
    [SerializeField] private float shakeStrength = 0.05f;

    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;

    private float originalChromatic;
    private float originalDistortion;

    private Coroutine pulseRoutine;

    private void Awake()
    {
        if (postProcessVolume == null)
            postProcessVolume = FindAnyObjectByType<Volume>();

        if (cameraShake == null)
            cameraShake = FindAnyObjectByType<CameraShakeController>();

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out lensDistortion);
        }

        if (chromaticAberration != null)
            originalChromatic = chromaticAberration.intensity.value;

        if (lensDistortion != null)
            originalDistortion = lensDistortion.intensity.value;
    }

    public void PlaySwitchPulse()
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(SwitchPulseRoutine());

        if (cameraShake != null)
            cameraShake.Shake(shakeDuration, shakeStrength);
    }

    private IEnumerator SwitchPulseRoutine()
    {
        float elapsed = 0f;

        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pulseDuration;

            // Fast spike, then smooth falloff
            float pulse = 1f - t;
            pulse *= pulse;

            if (chromaticAberration != null)
                chromaticAberration.intensity.value =
                    Mathf.Lerp(originalChromatic, chromaticIntensity, pulse);

            if (lensDistortion != null)
                lensDistortion.intensity.value =
                    Mathf.Lerp(originalDistortion, distortionIntensity, pulse);

            yield return null;
        }

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = originalChromatic;

        if (lensDistortion != null)
            lensDistortion.intensity.value = originalDistortion;

        pulseRoutine = null;
    }
}