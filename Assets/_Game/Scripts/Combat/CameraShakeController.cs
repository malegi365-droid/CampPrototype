using UnityEngine;
using System.Collections;

public class CameraShakeController : MonoBehaviour
{
    public static CameraShakeController Instance { get; private set; }

    [Header("Default Shake")]
    [SerializeField] private float defaultDuration = 0.08f;
    [SerializeField] private float defaultMagnitude = 0.75f;

    private Quaternion originalLocalRotation;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        Instance = this;
        originalLocalRotation = transform.localRotation;

        Debug.Log("CameraShakeController initialized on: " + gameObject.name);
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);

            transform.localRotation = originalLocalRotation * Quaternion.Euler(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = originalLocalRotation;
        shakeRoutine = null;
    }
}